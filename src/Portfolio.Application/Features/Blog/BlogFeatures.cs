using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using FluentValidation;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Contracts;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Domain.Exceptions;

namespace Portfolio.Application.Features.Blog;

public record PublicBlogSummaryDto(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string? CoverImageUrl,
    List<string> Tags,
    int ReadingTimeMinutes,
    DateTime? PublishedAtUtc
);

public record PublicBlogDetailDto(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string ContentMarkdown,
    string? CoverImageUrl,
    List<string> Tags,
    int ReadingTimeMinutes,
    DateTime? PublishedAtUtc
);

public record AdminBlogDto(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string ContentMarkdown,
    Guid? CoverImageAssetId,
    string? CoverImageUrl,
    List<string> Tags,
    int ReadingTimeMinutes,
    ContentStatus Status,
    DateTime? PublishedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record CreateBlogArticleCommand(
    string Slug,
    string Title,
    string Summary,
    string ContentMarkdown,
    Guid? CoverImageAssetId,
    List<string> Tags,
    ContentStatus Status
) : ICommand<AdminBlogDto>;

public record UpdateBlogArticleCommand(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string ContentMarkdown,
    Guid? CoverImageAssetId,
    List<string> Tags,
    ContentStatus Status
) : ICommand<AdminBlogDto>;

public record DeleteBlogArticleCommand(Guid Id) : ICommand;
public record ChangeBlogArticleStatusCommand(Guid Id, ContentStatus Status) : ICommand;

public class CreateBlogArticleCommandValidator : AbstractValidator<CreateBlogArticleCommand>
{
    public CreateBlogArticleCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ContentMarkdown).NotEmpty();
    }
}

public record GetPublicBlogPostsQuery(string? Tag) : IQuery<IList<PublicBlogSummaryDto>>;
public record GetPublicBlogPostBySlugQuery(string Slug) : IQuery<PublicBlogDetailDto?>;
public record GetAdminBlogPostsQuery(int PageNumber = 1, int PageSize = 20, ContentStatus? Status = null) : IQuery<PaginatedResult<AdminBlogDto>>;
public record GetAdminBlogPostByIdQuery(Guid Id) : IQuery<AdminBlogDto?>;

public class BlogFeatureHandlers :
    IQueryHandler<GetPublicBlogPostsQuery, IList<PublicBlogSummaryDto>>,
    IQueryHandler<GetPublicBlogPostBySlugQuery, PublicBlogDetailDto?>,
    IQueryHandler<GetAdminBlogPostsQuery, PaginatedResult<AdminBlogDto>>,
    IQueryHandler<GetAdminBlogPostByIdQuery, AdminBlogDto?>,
    ICommandHandler<CreateBlogArticleCommand, AdminBlogDto>,
    ICommandHandler<UpdateBlogArticleCommand, AdminBlogDto>,
    ICommandHandler<DeleteBlogArticleCommand>,
    ICommandHandler<ChangeBlogArticleStatusCommand>
{
    private readonly IApplicationUnitOfWork _unitOfWork;

    public BlogFeatureHandlers(IApplicationUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IList<PublicBlogSummaryDto>> Handle(GetPublicBlogPostsQuery request, CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.BlogArticles.GetPublishedArticlesAsync(request.Tag, cancellationToken);
        return list.Select(b => new PublicBlogSummaryDto(
            b.Id,
            b.Slug,
            b.Title,
            b.Summary,
            b.CoverImageAsset?.PublicUrl,
            b.Tags,
            b.ReadingTimeMinutes,
            b.PublishedAtUtc
        )).ToList();
    }

    public async Task<PublicBlogDetailDto?> Handle(GetPublicBlogPostBySlugQuery request, CancellationToken cancellationToken)
    {
        var b = await _unitOfWork.BlogArticles.GetPublishedBySlugAsync(request.Slug, cancellationToken);
        if (b == null) return null;

        return new PublicBlogDetailDto(
            b.Id,
            b.Slug,
            b.Title,
            b.Summary,
            b.ContentMarkdown,
            b.CoverImageAsset?.PublicUrl,
            b.Tags,
            b.ReadingTimeMinutes,
            b.PublishedAtUtc
        );
    }

    public async Task<PaginatedResult<AdminBlogDto>> Handle(GetAdminBlogPostsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.BlogArticles.Query();
        if (request.Status.HasValue)
            query = query.Where(b => b.Status == request.Status.Value);

        var total = query.Count();
        var items = query
            .OrderByDescending(b => b.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = items.Select(MapToAdmin).ToList();
        return PaginatedResult<AdminBlogDto>.Create(dtos, total, request.PageNumber, request.PageSize);
    }

    public async Task<AdminBlogDto?> Handle(GetAdminBlogPostByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.BlogArticles.GetByIdAsync(request.Id, cancellationToken);
        return item == null ? null : MapToAdmin(item);
    }

    public async Task<AdminBlogDto> Handle(CreateBlogArticleCommand request, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.BlogArticles.SlugExistsAsync(request.Slug, null, cancellationToken))
            throw new ConflictException($"Blog article with slug '{request.Slug}' already exists.");

        var wordCount = request.ContentMarkdown.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        var readingTime = Math.Max(1, (int)Math.Ceiling(wordCount / 200.0));

        var item = new BlogArticle
        {
            Id = Guid.NewGuid(),
            Slug = request.Slug.ToLowerInvariant().Trim(),
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            ContentMarkdown = request.ContentMarkdown,
            CoverImageAssetId = request.CoverImageAssetId,
            Tags = request.Tags ?? new(),
            ReadingTimeMinutes = readingTime,
            Status = request.Status,
            PublishedAtUtc = request.Status == ContentStatus.Published ? DateTime.UtcNow : null,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.BlogArticles.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdmin(item);
    }

    public async Task<AdminBlogDto> Handle(UpdateBlogArticleCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.BlogArticles.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(BlogArticle), request.Id);

        if (await _unitOfWork.BlogArticles.SlugExistsAsync(request.Slug, request.Id, cancellationToken))
            throw new ConflictException($"Blog article with slug '{request.Slug}' already exists.");

        var wordCount = request.ContentMarkdown.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        var readingTime = Math.Max(1, (int)Math.Ceiling(wordCount / 200.0));

        item.Slug = request.Slug.ToLowerInvariant().Trim();
        item.Title = request.Title.Trim();
        item.Summary = request.Summary.Trim();
        item.ContentMarkdown = request.ContentMarkdown;
        item.CoverImageAssetId = request.CoverImageAssetId;
        item.Tags = request.Tags ?? new();
        item.ReadingTimeMinutes = readingTime;

        if (item.Status != ContentStatus.Published && request.Status == ContentStatus.Published && item.PublishedAtUtc == null)
            item.PublishedAtUtc = DateTime.UtcNow;

        item.Status = request.Status;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdmin(item);
    }

    public async Task Handle(DeleteBlogArticleCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.BlogArticles.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(BlogArticle), request.Id);

        _unitOfWork.BlogArticles.Remove(item);
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(ChangeBlogArticleStatusCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.BlogArticles.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(BlogArticle), request.Id);

        if (request.Status == ContentStatus.Published && item.PublishedAtUtc == null)
            item.PublishedAtUtc = DateTime.UtcNow;

        item.Status = request.Status;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    private static AdminBlogDto MapToAdmin(BlogArticle b) => new(
        b.Id,
        b.Slug,
        b.Title,
        b.Summary,
        b.ContentMarkdown,
        b.CoverImageAssetId,
        b.CoverImageAsset?.PublicUrl,
        b.Tags,
        b.ReadingTimeMinutes,
        b.Status,
        b.PublishedAtUtc,
        b.CreatedAtUtc,
        b.UpdatedAtUtc
    );
}

