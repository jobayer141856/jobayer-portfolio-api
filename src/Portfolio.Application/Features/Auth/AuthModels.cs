namespace Portfolio.Application.Features.Auth;

public record LoginDto(string Email, string Password);
public record LoginResponseDto(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc, AdminUserDto User);
public record AdminUserDto(Guid Id, string Email, string DisplayName);
public record ChangePasswordDto(string CurrentPassword, string NewPassword);

