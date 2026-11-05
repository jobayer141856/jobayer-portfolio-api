using Portfolio.Domain.Contracts;
using Portfolio.Domain.ValueObjects;

namespace Portfolio.Domain.Entities;

public class Profile : IAggregateRoot<Guid>
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string ShortIntro { get; set; } = string.Empty;
    public string BiographyMarkdown { get; set; } = string.Empty;
    public string? WorkingApproachMarkdown { get; set; }
    public string? PublicEmail { get; set; }
    public bool ShowEmailPublicly { get; set; }
    public Guid? ProfileImageAssetId { get; set; }
    public MediaAsset? ProfileImageAsset { get; set; }
    public Guid? ResumePdfAssetId { get; set; }
    public MediaAsset? ResumePdfAsset { get; set; }
    public List<SocialLink> SocialLinks { get; set; } = new();
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

