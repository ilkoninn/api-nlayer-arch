namespace App.Business.DTOs.Service.Admin;

public class CreateServiceDto
{
    public string Title { get; set; } = null!;
    public string Excerpt { get; set; } = null!;
    public string Content { get; set; } = null!;
    public IFormFile? CoverImage { get; set; }
    public IFormFile? Icon { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }
}
