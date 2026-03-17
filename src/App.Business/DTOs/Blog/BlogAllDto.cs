namespace App.Business.DTOs.Blog;

public class BlogAllDto : BaseEntityDto 
{
    public string Slug { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;

    // Images
    public string? CoverImageUrl { get; set; }

    // Excerpt
    public string? Excerpt { get; set; }
    public IEnumerable<string>? Tags { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public IEnumerable<BlogCategoryDto>? Categories { get; set; }
}
