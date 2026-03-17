namespace App.Business.DTOs.ProjectCategory;

public class ProjectCategoryListQueryDto
{
    public string? Search { get; set; }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}