namespace App.Business.DTOs.BlogCategory;

public class BlogCategoryDto : BaseEntityDto
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
}
