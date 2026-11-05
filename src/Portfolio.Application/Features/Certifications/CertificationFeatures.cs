using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using FluentValidation;
using Portfolio.Application.Contracts;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Domain.Exceptions;

namespace Portfolio.Application.Features.Certifications;

public record PublicCertificationDto(
    Guid Id,
    string Title,
    string Issuer,
    DateOnly IssueDate,
    string? CredentialId,
    string? CredentialUrl,
    string? Description,
    string? BadgeAssetUrl,
    int DisplayOrder
);

public record AdminCertificationDto(
    Guid Id,
    string Title,
    string Issuer,
    DateOnly IssueDate,
    string? CredentialId,
    string? CredentialUrl,
    string? Description,
    Guid? BadgeAssetId,
    string? BadgeAssetUrl,
    int DisplayOrder,
    ContentStatus Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record CreateCertificationCommand(
    string Title,
    string Issuer,
    DateOnly IssueDate,
    string? CredentialId,
    string? CredentialUrl,
    string? Description,
    Guid? BadgeAssetId,
    int DisplayOrder,
    ContentStatus Status
) : ICommand<AdminCertificationDto>;

public record UpdateCertificationCommand(
    Guid Id,
    string Title,
    string Issuer,
    DateOnly IssueDate,
    string? CredentialId,
    string? CredentialUrl,
    string? Description,
    Guid? BadgeAssetId,
    int DisplayOrder,
    ContentStatus Status
) : ICommand<AdminCertificationDto>;

public record DeleteCertificationCommand(Guid Id) : ICommand;
public record ChangeCertificationStatusCommand(Guid Id, ContentStatus Status) : ICommand;
public record ReorderCertificationsCommand(List<Guid> OrderedIds) : ICommand;

public class CreateCertificationValidator : AbstractValidator<CreateCertificationCommand>
{
    public CreateCertificationValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Issuer).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IssueDate).NotEmpty();
    }
}

public record GetPublicCertificationsQuery : IQuery<IList<PublicCertificationDto>>;
public record GetAdminCertificationsQuery : IQuery<IList<AdminCertificationDto>>;
public record GetAdminCertificationByIdQuery(Guid Id) : IQuery<AdminCertificationDto?>;

public class CertificationFeatureHandlers :
    IQueryHandler<GetPublicCertificationsQuery, IList<PublicCertificationDto>>,
    IQueryHandler<GetAdminCertificationsQuery, IList<AdminCertificationDto>>,
    IQueryHandler<GetAdminCertificationByIdQuery, AdminCertificationDto?>,
    ICommandHandler<CreateCertificationCommand, AdminCertificationDto>,
    ICommandHandler<UpdateCertificationCommand, AdminCertificationDto>,
    ICommandHandler<DeleteCertificationCommand>,
    ICommandHandler<ChangeCertificationStatusCommand>,
    ICommandHandler<ReorderCertificationsCommand>
{
    private readonly IApplicationUnitOfWork _unitOfWork;

    public CertificationFeatureHandlers(IApplicationUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IList<PublicCertificationDto>> Handle(GetPublicCertificationsQuery request, CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.Certifications.GetPublishedOrderedAsync(cancellationToken);
        return list.Select(c => new PublicCertificationDto(
            c.Id,
            c.Title,
            c.Issuer,
            c.IssueDate,
            c.CredentialId,
            c.CredentialUrl,
            c.Description,
            c.BadgeAsset?.PublicUrl,
            c.DisplayOrder
        )).ToList();
    }

    public async Task<IList<AdminCertificationDto>> Handle(GetAdminCertificationsQuery request, CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.Certifications.GetAllAsync(cancellationToken);
        return list.OrderBy(c => c.DisplayOrder).Select(MapToAdmin).ToList();
    }

    public async Task<AdminCertificationDto?> Handle(GetAdminCertificationByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Certifications.GetByIdAsync(request.Id, cancellationToken);
        return item == null ? null : MapToAdmin(item);
    }

    public async Task<AdminCertificationDto> Handle(CreateCertificationCommand request, CancellationToken cancellationToken)
    {
        var item = new Certification
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Issuer = request.Issuer.Trim(),
            IssueDate = request.IssueDate,
            CredentialId = request.CredentialId,
            CredentialUrl = request.CredentialUrl,
            Description = request.Description,
            BadgeAssetId = request.BadgeAssetId,
            DisplayOrder = request.DisplayOrder,
            Status = request.Status,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Certifications.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdmin(item);
    }

    public async Task<AdminCertificationDto> Handle(UpdateCertificationCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Certifications.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(Certification), request.Id);

        item.Title = request.Title.Trim();
        item.Issuer = request.Issuer.Trim();
        item.IssueDate = request.IssueDate;
        item.CredentialId = request.CredentialId;
        item.CredentialUrl = request.CredentialUrl;
        item.Description = request.Description;
        item.BadgeAssetId = request.BadgeAssetId;
        item.DisplayOrder = request.DisplayOrder;
        item.Status = request.Status;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdmin(item);
    }

    public async Task Handle(DeleteCertificationCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Certifications.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(Certification), request.Id);

        _unitOfWork.Certifications.Remove(item);
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(ChangeCertificationStatusCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Certifications.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(Certification), request.Id);

        item.Status = request.Status;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(ReorderCertificationsCommand request, CancellationToken cancellationToken)
    {
        for (int i = 0; i < request.OrderedIds.Count; i++)
        {
            var item = await _unitOfWork.Certifications.GetByIdAsync(request.OrderedIds[i], cancellationToken);
            if (item != null)
            {
                item.DisplayOrder = i;
                item.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    private static AdminCertificationDto MapToAdmin(Certification c) => new(
        c.Id,
        c.Title,
        c.Issuer,
        c.IssueDate,
        c.CredentialId,
        c.CredentialUrl,
        c.Description,
        c.BadgeAssetId,
        c.BadgeAsset?.PublicUrl,
        c.DisplayOrder,
        c.Status,
        c.CreatedAtUtc,
        c.UpdatedAtUtc
    );
}

