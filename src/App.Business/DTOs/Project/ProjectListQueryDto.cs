namespace App.Business.DTOs.Project;

public class ProjectListQueryDto
{
    public string? Search { get; set; }
    public Guid? ProjectCategoryId { get; set; }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}