namespace App.Business.DTOs.Service;

public class ServiceAllDto : BaseEntityDto
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Excerpt { get; set; } = null!;
    public string CoverImageUrl { get; set; } = null!;
    public string IconUrl { get; set; } = null!;
}
