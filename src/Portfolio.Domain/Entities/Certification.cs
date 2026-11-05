using Portfolio.Domain.Contracts;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public class Certification : IAggregateRoot<Guid>, IAuditableEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public string? CredentialId { get; set; }
    public string? CredentialUrl { get; set; }
    public string? Description { get; set; }
    public Guid? BadgeAssetId { get; set; }
    public MediaAsset? BadgeAsset { get; set; }
    public int DisplayOrder { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

