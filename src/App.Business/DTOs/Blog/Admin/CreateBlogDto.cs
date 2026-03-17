namespace App.Business.DTOs.Blog.Admin;

public class CreateBlogDto
{
    public string Title { get; set; } = null!;
    public string? Excerpt { get; set; }
    public string Content { get; set; } = null!;

    public IEnumerable<Guid>? CategoryIds { get; set; }
    public IEnumerable<string>? Tags { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }

    // Images
    public IFormFile? CoverImage { get; set; }
    public IFormFile? OgImage { get; set; }

    // Status
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EBlogStatus Status { get; set; }
}
