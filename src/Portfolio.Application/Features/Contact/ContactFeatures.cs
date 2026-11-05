using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using FluentValidation;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Contracts;
using Portfolio.Application.Contracts.Services;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Domain.Exceptions;

namespace Portfolio.Application.Features.Contact;

public record SubmitContactCommand(
    string Name,
    string Email,
    string? Subject,
    string Message,
    string? Honeypot = null,
    string? IpHash = null,
    string? UserAgent = null
) : ICommand;

public record AdminContactMessageDto(
    Guid Id,
    string SenderName,
    string SenderEmail,
    string? Subject,
    string MessageBody,
    MessageStatus Status,
    string? IpHash,
    string? UserAgent,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc
);

public record ChangeContactMessageStatusCommand(Guid Id, MessageStatus Status) : ICommand;
public record DeleteContactMessageCommand(Guid Id) : ICommand;

public class SubmitContactCommandValidator : AbstractValidator<SubmitContactCommand>
{
    public SubmitContactCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Subject).MaximumLength(250);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
    }
}

public record GetAdminContactMessagesQuery(int PageNumber = 1, int PageSize = 20, MessageStatus? Status = null) : IQuery<PaginatedResult<AdminContactMessageDto>>;
public record GetAdminContactMessageByIdQuery(Guid Id) : IQuery<AdminContactMessageDto?>;

public class ContactFeatureHandlers :
    ICommandHandler<SubmitContactCommand>,
    ICommandHandler<ChangeContactMessageStatusCommand>,
    ICommandHandler<DeleteContactMessageCommand>,
    IQueryHandler<GetAdminContactMessagesQuery, PaginatedResult<AdminContactMessageDto>>,
    IQueryHandler<GetAdminContactMessageByIdQuery, AdminContactMessageDto?>
{
    private readonly IApplicationUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ContactFeatureHandlers(IApplicationUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task Handle(SubmitContactCommand request, CancellationToken cancellationToken)
    {
        // Honeypot check - if bot fills this hidden field, quietly accept without persisting or sending email
        if (!string.IsNullOrWhiteSpace(request.Honeypot))
            return;

        var msg = new ContactMessage
        {
            Id = Guid.NewGuid(),
            SenderName = request.Name.Trim(),
            SenderEmail = request.Email.Trim(),
            Subject = request.Subject?.Trim(),
            MessageBody = request.Message.Trim(),
            Status = MessageStatus.Unread,
            IpHash = request.IpHash,
            UserAgent = request.UserAgent,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.ContactMessages.AddAsync(msg, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        // Async email notification - non-blocking for visitor
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendContactNotificationAsync(msg, CancellationToken.None);
            }
            catch
            {
                // Never fail client response on email send error
            }
        }, CancellationToken.None);
    }

    public async Task Handle(ChangeContactMessageStatusCommand request, CancellationToken cancellationToken)
    {
        var msg = await _unitOfWork.ContactMessages.GetByIdAsync(request.Id, cancellationToken);
        if (msg == null)
            throw new NotFoundException(nameof(ContactMessage), request.Id);

        msg.Status = request.Status;
        if (request.Status == MessageStatus.Read && msg.ReadAtUtc == null)
            msg.ReadAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(DeleteContactMessageCommand request, CancellationToken cancellationToken)
    {
        var msg = await _unitOfWork.ContactMessages.GetByIdAsync(request.Id, cancellationToken);
        if (msg == null)
            throw new NotFoundException(nameof(ContactMessage), request.Id);

        _unitOfWork.ContactMessages.Remove(msg);
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task<PaginatedResult<AdminContactMessageDto>> Handle(GetAdminContactMessagesQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.ContactMessages.Query();
        if (request.Status.HasValue)
            query = query.Where(m => m.Status == request.Status.Value);

        var total = query.Count();
        var items = query
            .OrderByDescending(m => m.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = items.Select(MapToAdmin).ToList();
        return PaginatedResult<AdminContactMessageDto>.Create(dtos, total, request.PageNumber, request.PageSize);
    }

    public async Task<AdminContactMessageDto?> Handle(GetAdminContactMessageByIdQuery request, CancellationToken cancellationToken)
    {
        var msg = await _unitOfWork.ContactMessages.GetByIdAsync(request.Id, cancellationToken);
        if (msg == null) return null;

        if (msg.Status == MessageStatus.Unread)
        {
            msg.Status = MessageStatus.Read;
            msg.ReadAtUtc = DateTime.UtcNow;
            await _unitOfWork.SaveAsync(cancellationToken);
        }

        return MapToAdmin(msg);
    }

    private static AdminContactMessageDto MapToAdmin(ContactMessage m) => new(
        m.Id,
        m.SenderName,
        m.SenderEmail,
        m.Subject,
        m.MessageBody,
        m.Status,
        m.IpHash,
        m.UserAgent,
        m.CreatedAtUtc,
        m.ReadAtUtc
    );
}

