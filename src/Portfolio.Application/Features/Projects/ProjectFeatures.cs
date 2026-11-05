using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using FluentValidation;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Contracts;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Domain.Exceptions;
using DomainValidationException = Portfolio.Domain.Exceptions.ValidationException;

namespace Portfolio.Application.Features.Projects;

public record PublicProjectSummaryDto(
    Guid Id,
    string Slug,
    string Title,
    string ShortSummary,
    string Category,
    List<string> TechnologyTags,
    string? CoverImageUrl,
    string? LiveDemoUrl,
    string? RepositoryUrl,
    bool IsFeatured,
    int DisplayOrder,
    DateTime? PublishedAtUtc
);

public record ProjectGalleryImageDto(Guid Id, Guid MediaAssetId, string PublicUrl, string? Caption, int DisplayOrder);

public record PublicProjectDetailDto(
    Guid Id,
    string Slug,
    string Title,
    string ShortSummary,
    string Category,
    string? ProblemContextMarkdown,
    string? GoalsMarkdown,
    string? RoleContributionsMarkdown,
    string? ArchitectureOverviewMarkdown,
    string? ChallengesDecisionsMarkdown,
    string? OutcomesMarkdown,
    List<string> TechnologyTags,
    string? CoverImageUrl,
    string? LiveDemoUrl,
    string? RepositoryUrl,
    bool IsFeatured,
    List<ProjectGalleryImageDto> GalleryImages,
    string? SeoTitle,
    string? SeoDescription,
    DateTime? PublishedAtUtc
);

public record AdminProjectDto(
    Guid Id,
    string Slug,
    string Title,
    string ShortSummary,
    string Category,
    string? ProblemContextMarkdown,
    string? GoalsMarkdown,
    string? RoleContributionsMarkdown,
    string? ArchitectureOverviewMarkdown,
    string? ChallengesDecisionsMarkdown,
    string? OutcomesMarkdown,
    List<string> TechnologyTags,
    Guid? CoverImageAssetId,
    string? CoverImageUrl,
    string? LiveDemoUrl,
    string? RepositoryUrl,
    bool IsFeatured,
    int DisplayOrder,
    ContentStatus Status,
    List<ProjectGalleryImageDto> GalleryImages,
    string? SeoTitle,
    string? SeoDescription,
    DateTime? PublishedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record CreateProjectCommand(
    string Slug,
    string Title,
    string ShortSummary,
    string Category,
    string? ProblemContextMarkdown,
    string? GoalsMarkdown,
    string? RoleContributionsMarkdown,
    string? ArchitectureOverviewMarkdown,
    string? ChallengesDecisionsMarkdown,
    string? OutcomesMarkdown,
    List<string> TechnologyTags,
    Guid? CoverImageAssetId,
    string? LiveDemoUrl,
    string? RepositoryUrl,
    bool IsFeatured,
    int DisplayOrder,
    ContentStatus Status,
    string? SeoTitle,
    string? SeoDescription
) : ICommand<AdminProjectDto>;

public record UpdateProjectCommand(
    Guid Id,
    string Slug,
    string Title,
    string ShortSummary,
    string Category,
    string? ProblemContextMarkdown,
    string? GoalsMarkdown,
    string? RoleContributionsMarkdown,
    string? ArchitectureOverviewMarkdown,
    string? ChallengesDecisionsMarkdown,
    string? OutcomesMarkdown,
    List<string> TechnologyTags,
    Guid? CoverImageAssetId,
    string? LiveDemoUrl,
    string? RepositoryUrl,
    bool IsFeatured,
    int DisplayOrder,
    ContentStatus Status,
    string? SeoTitle,
    string? SeoDescription
) : ICommand<AdminProjectDto>;

public record DeleteProjectCommand(Guid Id) : ICommand;
public record ChangeProjectStatusCommand(Guid Id, ContentStatus Status) : ICommand;
public record ReorderProjectsCommand(List<Guid> OrderedIds) : ICommand;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must consist of lowercase letters, numbers, and hyphens.");
        RuleFor(x => x.ShortSummary).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        When(x => x.Status == ContentStatus.Published, () =>
        {
            RuleFor(x => x.ShortSummary).NotEmpty();
            RuleFor(x => x.CoverImageAssetId).NotNull().WithMessage("Published project requires a cover image.");
        });
    }
}

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must consist of lowercase letters, numbers, and hyphens.");
        RuleFor(x => x.ShortSummary).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        When(x => x.Status == ContentStatus.Published, () =>
        {
            RuleFor(x => x.ShortSummary).NotEmpty();
            RuleFor(x => x.CoverImageAssetId).NotNull().WithMessage("Published project requires a cover image.");
        });
    }
}

public record GetPublicProjectsQuery(bool? Featured, string? Category, string? Tag) : IQuery<IList<PublicProjectSummaryDto>>;
public record GetPublicProjectBySlugQuery(string Slug) : IQuery<PublicProjectDetailDto?>;
public record GetAdminProjectsQuery(int PageNumber = 1, int PageSize = 20, ContentStatus? Status = null) : IQuery<PaginatedResult<AdminProjectDto>>;
public record GetAdminProjectByIdQuery(Guid Id) : IQuery<AdminProjectDto?>;

public class ProjectFeatureHandlers :
    IQueryHandler<GetPublicProjectsQuery, IList<PublicProjectSummaryDto>>,
    IQueryHandler<GetPublicProjectBySlugQuery, PublicProjectDetailDto?>,
    IQueryHandler<GetAdminProjectsQuery, PaginatedResult<AdminProjectDto>>,
    IQueryHandler<GetAdminProjectByIdQuery, AdminProjectDto?>,
    ICommandHandler<CreateProjectCommand, AdminProjectDto>,
    ICommandHandler<UpdateProjectCommand, AdminProjectDto>,
    ICommandHandler<DeleteProjectCommand>,
    ICommandHandler<ChangeProjectStatusCommand>,
    ICommandHandler<ReorderProjectsCommand>
{
    private readonly IApplicationUnitOfWork _unitOfWork;

    public ProjectFeatureHandlers(IApplicationUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IList<PublicProjectSummaryDto>> Handle(GetPublicProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _unitOfWork.Projects.GetPublishedProjectsAsync(request.Featured, request.Category, request.Tag, cancellationToken);
        return projects.Select(p => new PublicProjectSummaryDto(
            p.Id,
            p.Slug,
            p.Title,
            p.ShortSummary,
            p.Category,
            p.TechnologyTags,
            p.CoverImageAsset?.PublicUrl,
            p.LiveDemoUrl,
            p.RepositoryUrl,
            p.IsFeatured,
            p.DisplayOrder,
            p.PublishedAtUtc
        )).ToList();
    }

    public async Task<PublicProjectDetailDto?> Handle(GetPublicProjectBySlugQuery request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetPublishedBySlugAsync(request.Slug, cancellationToken);
        if (project == null) return null;

        return new PublicProjectDetailDto(
            project.Id,
            project.Slug,
            project.Title,
            project.ShortSummary,
            project.Category,
            project.ProblemContextMarkdown,
            project.GoalsMarkdown,
            project.RoleContributionsMarkdown,
            project.ArchitectureOverviewMarkdown,
            project.ChallengesDecisionsMarkdown,
            project.OutcomesMarkdown,
            project.TechnologyTags,
            project.CoverImageAsset?.PublicUrl,
            project.LiveDemoUrl,
            project.RepositoryUrl,
            project.IsFeatured,
            project.GalleryImages.OrderBy(g => g.DisplayOrder).Select(g => new ProjectGalleryImageDto(
                g.Id, g.MediaAssetId, g.MediaAsset?.PublicUrl ?? string.Empty, g.Caption, g.DisplayOrder
            )).ToList(),
            project.SeoTitle,
            project.SeoDescription,
            project.PublishedAtUtc
        );
    }

    public async Task<PaginatedResult<AdminProjectDto>> Handle(GetAdminProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Projects.Query();
        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        var total = query.Count();
        var items = query
            .OrderBy(p => p.DisplayOrder)
            .ThenByDescending(p => p.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = items.Select(MapToAdmin).ToList();
        return PaginatedResult<AdminProjectDto>.Create(dtos, total, request.PageNumber, request.PageSize);
    }

    public async Task<AdminProjectDto?> Handle(GetAdminProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(request.Id, cancellationToken);
        return project == null ? null : MapToAdmin(project);
    }

    public async Task<AdminProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var validation = await new CreateProjectCommandValidator().ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new DomainValidationException(validation.ToDictionary());

        if (await _unitOfWork.Projects.SlugExistsAsync(request.Slug, null, cancellationToken))
            throw new ConflictException($"Project with slug '{request.Slug}' already exists.");

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Slug = request.Slug.ToLowerInvariant().Trim(),
            Title = request.Title.Trim(),
            ShortSummary = request.ShortSummary.Trim(),
            Category = request.Category.Trim(),
            ProblemContextMarkdown = request.ProblemContextMarkdown,
            GoalsMarkdown = request.GoalsMarkdown,
            RoleContributionsMarkdown = request.RoleContributionsMarkdown,
            ArchitectureOverviewMarkdown = request.ArchitectureOverviewMarkdown,
            ChallengesDecisionsMarkdown = request.ChallengesDecisionsMarkdown,
            OutcomesMarkdown = request.OutcomesMarkdown,
            TechnologyTags = request.TechnologyTags ?? new(),
            CoverImageAssetId = request.CoverImageAssetId,
            LiveDemoUrl = request.LiveDemoUrl,
            RepositoryUrl = request.RepositoryUrl,
            IsFeatured = request.IsFeatured,
            DisplayOrder = request.DisplayOrder,
            Status = request.Status,
            PublishedAtUtc = request.Status == ContentStatus.Published ? DateTime.UtcNow : null,
            SeoTitle = request.SeoTitle,
            SeoDescription = request.SeoDescription,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Projects.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdmin(project);
    }

    public async Task<AdminProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var validation = await new UpdateProjectCommandValidator().ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new DomainValidationException(validation.ToDictionary());

        var project = await _unitOfWork.Projects.GetByIdAsync(request.Id, cancellationToken);
        if (project == null)
            throw new NotFoundException(nameof(Project), request.Id);

        if (await _unitOfWork.Projects.SlugExistsAsync(request.Slug, request.Id, cancellationToken))
            throw new ConflictException($"Project with slug '{request.Slug}' already exists.");

        project.Slug = request.Slug.ToLowerInvariant().Trim();
        project.Title = request.Title.Trim();
        project.ShortSummary = request.ShortSummary.Trim();
        project.Category = request.Category.Trim();
        project.ProblemContextMarkdown = request.ProblemContextMarkdown;
        project.GoalsMarkdown = request.GoalsMarkdown;
        project.RoleContributionsMarkdown = request.RoleContributionsMarkdown;
        project.ArchitectureOverviewMarkdown = request.ArchitectureOverviewMarkdown;
        project.ChallengesDecisionsMarkdown = request.ChallengesDecisionsMarkdown;
        project.OutcomesMarkdown = request.OutcomesMarkdown;
        project.TechnologyTags = request.TechnologyTags ?? new();
        project.CoverImageAssetId = request.CoverImageAssetId;
        project.LiveDemoUrl = request.LiveDemoUrl;
        project.RepositoryUrl = request.RepositoryUrl;
        project.IsFeatured = request.IsFeatured;
        project.DisplayOrder = request.DisplayOrder;

        if (project.Status != ContentStatus.Published && request.Status == ContentStatus.Published && project.PublishedAtUtc == null)
            project.PublishedAtUtc = DateTime.UtcNow;

        project.Status = request.Status;
        project.SeoTitle = request.SeoTitle;
        project.SeoDescription = request.SeoDescription;
        project.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdmin(project);
    }

    public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(request.Id, cancellationToken);
        if (project == null)
            throw new NotFoundException(nameof(Project), request.Id);

        _unitOfWork.Projects.Remove(project);
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(ChangeProjectStatusCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(request.Id, cancellationToken);
        if (project == null)
            throw new NotFoundException(nameof(Project), request.Id);

        if (request.Status == ContentStatus.Published && project.PublishedAtUtc == null)
            project.PublishedAtUtc = DateTime.UtcNow;

        project.Status = request.Status;
        project.UpdatedAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(ReorderProjectsCommand request, CancellationToken cancellationToken)
    {
        for (int i = 0; i < request.OrderedIds.Count; i++)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(request.OrderedIds[i], cancellationToken);
            if (project != null)
            {
                project.DisplayOrder = i;
                project.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    private static AdminProjectDto MapToAdmin(Project p) => new(
        p.Id,
        p.Slug,
        p.Title,
        p.ShortSummary,
        p.Category,
        p.ProblemContextMarkdown,
        p.GoalsMarkdown,
        p.RoleContributionsMarkdown,
        p.ArchitectureOverviewMarkdown,
        p.ChallengesDecisionsMarkdown,
        p.OutcomesMarkdown,
        p.TechnologyTags,
        p.CoverImageAssetId,
        p.CoverImageAsset?.PublicUrl,
        p.LiveDemoUrl,
        p.RepositoryUrl,
        p.IsFeatured,
        p.DisplayOrder,
        p.Status,
        p.GalleryImages.Select(g => new ProjectGalleryImageDto(g.Id, g.MediaAssetId, g.MediaAsset?.PublicUrl ?? string.Empty, g.Caption, g.DisplayOrder)).ToList(),
        p.SeoTitle,
        p.SeoDescription,
        p.PublishedAtUtc,
        p.CreatedAtUtc,
        p.UpdatedAtUtc
    );
}

