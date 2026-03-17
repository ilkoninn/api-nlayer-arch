namespace App.Business.DTOs.BlogCategory;

public class BlogCategoryListQueryDto
{
    public string? Search { get; set; }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
