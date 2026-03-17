namespace App.Business.DTOs.Project;

public class ProjectAllDto : BaseEntityDto
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Excerpt { get; set; } = null!;
    public ProjectCategoryDto Category { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
}