using Portfolio.Domain.Contracts;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public class BlogArticle : IAggregateRoot<Guid>, IAuditableEntity
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ContentMarkdown { get; set; } = string.Empty;
    public Guid? CoverImageAssetId { get; set; }
    public MediaAsset? CoverImageAsset { get; set; }
    public List<string> Tags { get; set; } = new();
    public int ReadingTimeMinutes { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

