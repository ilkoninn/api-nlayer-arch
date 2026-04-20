namespace App.Business.DTOs.Blog;

public class BlogListQueryDto
{
    public string? Search { get; set; }
    public EBlogStatus? Status { get; set; }
    public Guid? CategoryId { get; set; }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
