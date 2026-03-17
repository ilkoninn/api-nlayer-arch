namespace App.Core.Entities.Projects;

public class ProjectCategory : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
