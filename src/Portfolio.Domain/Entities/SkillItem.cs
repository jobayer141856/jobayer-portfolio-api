using Portfolio.Domain.Contracts;

namespace Portfolio.Domain.Entities;

public class SkillItem : IAggregateRoot<Guid>
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public SkillCategory? Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContextDescription { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}

