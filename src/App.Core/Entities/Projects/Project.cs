namespace App.Core.Entities.Projects;

public class Project : AuditableEntity
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Excerpt { get; set; } = null!;
    public string Content { get; set; } = null!;

    // SEO Fields
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }

    public Guid ProjectCategoryId { get; set; } 
    public ProjectCategory ProjectCategory { get; set; } = null!;

    public ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();
    public ICollection<ProjectFeature> Features { get; set; } = new List<ProjectFeature>();
}
