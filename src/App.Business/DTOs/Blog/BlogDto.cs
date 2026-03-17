namespace App.Business.DTOs.Blog;

public class BlogDto : BaseEntityDto
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Content { get; set; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EBlogStatus Status { get; set; }

    public string? Excerpt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }

    // SEO Fields
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }

    // Tags as JSON array - user-defined, SEO purposes
    public IEnumerable<string>? Tags { get; set; }

    // Images
    public string CoverImageUrl { get; set; } = null!;
    public string? OgImageUrl { get; set; }

    // Relations
    public IEnumerable<BlogCategoryDto>? Categories { get; set; }
}
