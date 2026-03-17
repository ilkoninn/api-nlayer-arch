namespace App.Core.Entities.Projects;

public class ProjectFeature : AuditableEntity
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}
