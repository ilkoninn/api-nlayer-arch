namespace App.Business.DTOs.ProjectCategory;

public class ProjectCategoryDto : BaseEntityDto
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
}