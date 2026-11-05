using Portfolio.Domain.Contracts;
using Portfolio.Domain.Enums;
using Portfolio.Domain.ValueObjects;

namespace Portfolio.Domain.Entities;

public class Publication : IAggregateRoot<Guid>, IAuditableEntity
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<AuthorCitation> Authors { get; set; } = new();
    public string Venue { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public DateOnly? PublicationDate { get; set; }
    public string? Doi { get; set; }
    public string? OfficialUrl { get; set; }
    public Guid? PdfAssetId { get; set; }
    public MediaAsset? PdfAsset { get; set; }
    public string? AbstractText { get; set; }
    public int DisplayOrder { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

