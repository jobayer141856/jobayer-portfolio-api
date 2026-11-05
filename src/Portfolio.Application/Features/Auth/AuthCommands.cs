using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using FluentValidation;
using Portfolio.Application.Contracts;
using Portfolio.Application.Contracts.Services;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Exceptions;
using DomainValidationException = Portfolio.Domain.Exceptions.ValidationException;

namespace Portfolio.Application.Features.Auth;

public record LoginCommand(string Email, string Password, string? IpAddress) : ICommand<LoginResponseDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponseDto>
{
    private readonly IApplicationUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IApplicationUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await new LoginCommandValidator().ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainValidationException(validationResult.ToDictionary());

        var user = await _unitOfWork.AdminUsers.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
            throw new DomainValidationException(new Dictionary<string, string[]> { { "Credentials", new[] { "Invalid email or password." } } });

        if (user.LockoutEndUtc != null && user.LockoutEndUtc > DateTime.UtcNow)
            throw new DomainValidationException(new Dictionary<string, string[]> { { "Account", new[] { $"Account is locked until {user.LockoutEndUtc} UTC." } } });

        bool isValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isValid)
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= 5)
            {
                user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginCount = 0;
            }
            await _unitOfWork.SaveAsync(cancellationToken);
            throw new DomainValidationException(new Dictionary<string, string[]> { { "Credentials", new[] { "Invalid email or password." } } });
        }

        user.FailedLoginCount = 0;
        user.LockoutEndUtc = null;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id, request.IpAddress);

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return new LoginResponseDto(
            accessToken,
            refreshToken.TokenHash,
            refreshToken.ExpiresAtUtc,
            new AdminUserDto(user.Id, user.Email, user.DisplayName)
        );
    }
}

public record RefreshTokenCommand(string RawToken, string? IpAddress) : ICommand<LoginResponseDto>;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, LoginResponseDto>
{
    private readonly IApplicationUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IApplicationUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hashedToken = _tokenService.HashToken(request.RawToken);
        var token = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(hashedToken, cancellationToken);

        if (token == null)
            throw new DomainValidationException(new Dictionary<string, string[]> { { "Token", new[] { "Invalid refresh token." } } });

        if (token.IsRevoked)
        {
            await _unitOfWork.RefreshTokens.RevokeFamilyAsync(token.AdminUserId, "Reused revoked token", cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            throw new DomainValidationException(new Dictionary<string, string[]> { { "Token", new[] { "Compromised refresh token reused. Session revoked." } } });
        }

        if (token.IsExpired)
            throw new DomainValidationException(new Dictionary<string, string[]> { { "Token", new[] { "Refresh token has expired." } } });

        var user = await _unitOfWork.AdminUsers.GetByIdAsync(token.AdminUserId, cancellationToken);
        if (user == null)
            throw new NotFoundException(nameof(AdminUser), token.AdminUserId);

        var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id, request.IpAddress);
        token.RevokedAtUtc = DateTime.UtcNow;
        token.ReplacedByTokenHash = newRefreshToken.TokenHash;

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);

        return new LoginResponseDto(
            accessToken,
            newRefreshToken.TokenHash,
            newRefreshToken.ExpiresAtUtc,
            new AdminUserDto(user.Id, user.Email, user.DisplayName)
        );
    }
}

public record LogoutCommand(string? RawToken) : ICommand;

public class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly IApplicationUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public LogoutCommandHandler(IApplicationUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RawToken)) return;

        var hashed = _tokenService.HashToken(request.RawToken);
        var token = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(hashed, cancellationToken);
        if (token != null && !token.IsRevoked)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            await _unitOfWork.SaveAsync(cancellationToken);
        }
    }
}

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : ICommand;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IApplicationUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(IApplicationUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await new ChangePasswordCommandValidator().ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainValidationException(validationResult.ToDictionary());

        var user = await _unitOfWork.AdminUsers.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException(nameof(AdminUser), request.UserId);

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new DomainValidationException(new Dictionary<string, string[]> { { "CurrentPassword", new[] { "Incorrect current password." } } });

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveAsync(cancellationToken);
    }
}

public record GetMeQuery(Guid UserId) : IQuery<AdminUserDto>;

public class GetMeQueryHandler : IQueryHandler<GetMeQuery, AdminUserDto>
{
    private readonly IApplicationUnitOfWork _unitOfWork;

    public GetMeQueryHandler(IApplicationUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AdminUserDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AdminUsers.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException(nameof(AdminUser), request.UserId);

        return new AdminUserDto(user.Id, user.Email, user.DisplayName);
    }
}

