using Portfolio.Domain.Contracts;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public class Project : IAggregateRoot<Guid>, IAuditableEntity
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ShortSummary { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ProblemContextMarkdown { get; set; }
    public string? GoalsMarkdown { get; set; }
    public string? RoleContributionsMarkdown { get; set; }
    public string? ArchitectureOverviewMarkdown { get; set; }
    public string? ChallengesDecisionsMarkdown { get; set; }
    public string? OutcomesMarkdown { get; set; }
    public List<string> TechnologyTags { get; set; } = new();
    public Guid? CoverImageAssetId { get; set; }
    public MediaAsset? CoverImageAsset { get; set; }
    public string? LiveDemoUrl { get; set; }
    public string? RepositoryUrl { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime? PublishedAtUtc { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<ProjectImage> GalleryImages { get; set; } = new List<ProjectImage>();
}

