using Portfolio.Domain.Contracts;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public class Education : IAggregateRoot<Guid>, IAuditableEntity
{
    public Guid Id { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? ResultText { get; set; }
    public bool ShowResultPublicly { get; set; }
    public string? Description { get; set; }
    public string? InstitutionUrl { get; set; }
    public int DisplayOrder { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

