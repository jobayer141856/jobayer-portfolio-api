using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using FluentValidation;
using Portfolio.Application.Contracts;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Domain.Exceptions;
using Portfolio.Domain.ValueObjects;

namespace Portfolio.Application.Features.Publications;

public record PublicPublicationDto(
    Guid Id,
    string Slug,
    string Title,
    List<AuthorCitation> Authors,
    string Venue,
    int PublicationYear,
    DateOnly? PublicationDate,
    string? Doi,
    string? OfficialUrl,
    string? PdfAssetUrl,
    string? AbstractText,
    int DisplayOrder
);

public record AdminPublicationDto(
    Guid Id,
    string Slug,
    string Title,
    List<AuthorCitation> Authors,
    string Venue,
    int PublicationYear,
    DateOnly? PublicationDate,
    string? Doi,
    string? OfficialUrl,
    Guid? PdfAssetId,
    string? PdfAssetUrl,
    string? AbstractText,
    int DisplayOrder,
    ContentStatus Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record CreatePublicationCommand(
    string Slug,
    string Title,
    List<AuthorCitation> Authors,
    string Venue,
    int PublicationYear,
    DateOnly? PublicationDate,
    string? Doi,
    string? OfficialUrl,
    Guid? PdfAssetId,
    string? AbstractText,
    int DisplayOrder,
    ContentStatus Status
) : ICommand<AdminPublicationDto>;

public record UpdatePublicationCommand(
    Guid Id,
    string Slug,
    string Title,
    List<AuthorCitation> Authors,
    string Venue,
    int PublicationYear,
    DateOnly? PublicationDate,
    string? Doi,
    string? OfficialUrl,
    Guid? PdfAssetId,
    string? AbstractText,
    int DisplayOrder,
    ContentStatus Status
) : ICommand<AdminPublicationDto>;

public record DeletePublicationCommand(Guid Id) : ICommand;
public record ChangePublicationStatusCommand(Guid Id, ContentStatus Status) : ICommand;
public record ReorderPublicationsCommand(List<Guid> OrderedIds) : ICommand;

public class CreatePublicationValidator : AbstractValidator<CreatePublicationCommand>
{
    public CreatePublicationValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(400);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Venue).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PublicationYear).InclusiveBetween(1900, 2100);
    }
}

public record GetPublicPublicationsQuery : IQuery<IList<PublicPublicationDto>>;
public record GetAdminPublicationsQuery : IQuery<IList<AdminPublicationDto>>;
public record GetAdminPublicationByIdQuery(Guid Id) : IQuery<AdminPublicationDto?>;

public class PublicationFeatureHandlers :
    IQueryHandler<GetPublicPublicationsQuery, IList<PublicPublicationDto>>,
    IQueryHandler<GetAdminPublicationsQuery, IList<AdminPublicationDto>>,
    IQueryHandler<GetAdminPublicationByIdQuery, AdminPublicationDto?>,
    ICommandHandler<CreatePublicationCommand, AdminPublicationDto>,
    ICommandHandler<UpdatePublicationCommand, AdminPublicationDto>,
    ICommandHandler<DeletePublicationCommand>,
    ICommandHandler<ChangePublicationStatusCommand>,
    ICommandHandler<ReorderPublicationsCommand>
{
    private readonly IApplicationUnitOfWork _unitOfWork;

    public PublicationFeatureHandlers(IApplicationUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IList<PublicPublicationDto>> Handle(GetPublicPublicationsQuery request, CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.Publications.GetPublishedOrderedAsync(cancellationToken);
        return list.Select(p => new PublicPublicationDto(
            p.Id,
            p.Slug,
            p.Title,
            p.Authors.OrderBy(a => a.Order).ToList(),
            p.Venue,
            p.PublicationYear,
            p.PublicationDate,
            p.Doi,
            p.OfficialUrl,
            p.PdfAsset?.PublicUrl,
            p.AbstractText,
            p.DisplayOrder
        )).ToList();
    }

    public async Task<IList<AdminPublicationDto>> Handle(GetAdminPublicationsQuery request, CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.Publications.GetAllAsync(cancellationToken);
        return list.OrderBy(p => p.DisplayOrder).Select(MapToAdmin).ToList();
    }

    public async Task<AdminPublicationDto?> Handle(GetAdminPublicationByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Publications.GetByIdAsync(request.Id, cancellationToken);
        return item == null ? null : MapToAdmin(item);
    }

    public async Task<AdminPublicationDto> Handle(CreatePublicationCommand request, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Publications.GetBySlugAsync(request.Slug, cancellationToken);
        if (existing != null)
            throw new ConflictException($"Publication with slug '{request.Slug}' already exists.");

        var item = new Publication
        {
            Id = Guid.NewGuid(),
            Slug = request.Slug.ToLowerInvariant().Trim(),
            Title = request.Title.Trim(),
            Authors = request.Authors ?? new(),
            Venue = request.Venue.Trim(),
            PublicationYear = request.PublicationYear,
            PublicationDate = request.PublicationDate,
            Doi = request.Doi,
            OfficialUrl = request.OfficialUrl,
            PdfAssetId = request.PdfAssetId,
            AbstractText = request.AbstractText,
            DisplayOrder = request.DisplayOrder,
            Status = request.Status,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Publications.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdmin(item);
    }

    public async Task<AdminPublicationDto> Handle(UpdatePublicationCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Publications.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(Publication), request.Id);

        var existingSlug = await _unitOfWork.Publications.GetBySlugAsync(request.Slug, cancellationToken);
        if (existingSlug != null && existingSlug.Id != request.Id)
            throw new ConflictException($"Publication with slug '{request.Slug}' already exists.");

        item.Slug = request.Slug.ToLowerInvariant().Trim();
        item.Title = request.Title.Trim();
        item.Authors = request.Authors ?? new();
        item.Venue = request.Venue.Trim();
        item.PublicationYear = request.PublicationYear;
        item.PublicationDate = request.PublicationDate;
        item.Doi = request.Doi;
        item.OfficialUrl = request.OfficialUrl;
        item.PdfAssetId = request.PdfAssetId;
        item.AbstractText = request.AbstractText;
        item.DisplayOrder = request.DisplayOrder;
        item.Status = request.Status;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdmin(item);
    }

    public async Task Handle(DeletePublicationCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Publications.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(Publication), request.Id);

        _unitOfWork.Publications.Remove(item);
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(ChangePublicationStatusCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Publications.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(Publication), request.Id);

        item.Status = request.Status;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(ReorderPublicationsCommand request, CancellationToken cancellationToken)
    {
        for (int i = 0; i < request.OrderedIds.Count; i++)
        {
            var item = await _unitOfWork.Publications.GetByIdAsync(request.OrderedIds[i], cancellationToken);
            if (item != null)
            {
                item.DisplayOrder = i;
                item.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    private static AdminPublicationDto MapToAdmin(Publication p) => new(
        p.Id,
        p.Slug,
        p.Title,
        p.Authors,
        p.Venue,
        p.PublicationYear,
        p.PublicationDate,
        p.Doi,
        p.OfficialUrl,
        p.PdfAssetId,
        p.PdfAsset?.PublicUrl,
        p.AbstractText,
        p.DisplayOrder,
        p.Status,
        p.CreatedAtUtc,
        p.UpdatedAtUtc
    );
}

