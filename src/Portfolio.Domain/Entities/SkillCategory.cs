using Portfolio.Domain.Contracts;

namespace Portfolio.Domain.Entities;

public class SkillCategory : IAggregateRoot<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;

    public ICollection<SkillItem> Items { get; set; } = new List<SkillItem>();
}

