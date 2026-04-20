namespace App.Business.DTOs.Service;

public class ServiceDto : BaseEntityDto
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Excerpt { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string CoverImageUrl { get; set; } = null!;
    public string IconUrl { get; set; } = null!;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }
}
