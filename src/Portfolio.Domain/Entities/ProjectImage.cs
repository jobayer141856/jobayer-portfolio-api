using Portfolio.Domain.Contracts;

namespace Portfolio.Domain.Entities;

public class ProjectImage : IAggregateRoot<Guid>
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public Guid MediaAssetId { get; set; }
    public MediaAsset? MediaAsset { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
}

