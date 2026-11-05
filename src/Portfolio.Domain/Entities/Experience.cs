using Portfolio.Domain.Contracts;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public class Experience : IAggregateRoot<Guid>, IAuditableEntity
{
    public Guid Id { get; set; }
    public string Organization { get; set; } = string.Empty;
    public Guid? OrganizationLogoAssetId { get; set; }
    public MediaAsset? OrganizationLogoAsset { get; set; }
    public string RoleTitle { get; set; } = string.Empty;
    public string? EmploymentType { get; set; }
    public string? Location { get; set; }
    public string? WorkArrangement { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<string> Responsibilities { get; set; } = new();
    public List<string> Outcomes { get; set; } = new();
    public List<string> TechnologyTags { get; set; } = new();
    public int DisplayOrder { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

