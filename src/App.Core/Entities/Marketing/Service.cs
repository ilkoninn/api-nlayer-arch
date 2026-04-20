namespace App.Core.Entities;

public class Service : AuditableEntity
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Excerpt { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
    public string? IconUrl { get; set; }


    // SEO Fields
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }
}
