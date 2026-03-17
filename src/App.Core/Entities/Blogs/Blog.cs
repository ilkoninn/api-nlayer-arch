namespace App.Core.Entities.Blogs;

public class Blog : AuditableEntity
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Content { get; set; } = null!;
    public EBlogStatus Status { get; set; }

    public string? Excerpt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }

    // SEO Fields
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }

    // Tags as JSON array - user-defined, SEO purposes
    public string? Tags { get; set; }

    // Images
    public string? CoverImageUrl { get; set; }
    public string? OgImageUrl { get; set; }

    // Relations
    public ICollection<BlogCategory> BlogCategories { get; set; } = new List<BlogCategory>();
}
