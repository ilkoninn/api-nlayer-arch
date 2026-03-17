namespace App.Core.Entities.Projects;

public class ProjectImage : AuditableEntity
{
    public string ImageUrl { get; set; } = null!;

    public Guid ProjectId { get; set; } 
    public Project Project { get; set; } = null!;
}
