namespace App.Business.DTOs.Project;

public class ProjectDto : BaseEntityDto
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Excerpt { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }
    public ProjectCategoryDto Category { get; set; } = null!;
    public IEnumerable<ProjectImageDto>? Images { get; set; }
    public IEnumerable<ProjectFeatureDto>? Features { get; set; }
}