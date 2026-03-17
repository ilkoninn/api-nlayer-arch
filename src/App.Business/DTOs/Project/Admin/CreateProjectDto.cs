namespace App.Business.DTOs.Project.Admin;

public class CreateProjectDto
{
    public string Title { get; set; } = null!;
    public string Excerpt { get; set; } = null!;
    public string Content { get; set; } = null!;
    public Guid ProjectCategoryId { get; set; }
    public List<IFormFile> Images { get; set; } = new();
    
    // JSON string from form, will be deserialized in controller
    public string? FeaturesJson { get; set; }

    // Populated after deserialization
    [SwaggerExclude]
    [JsonIgnore]
    [BindNever]
    public List<ProjectFeatureInputDto>? Features { get; set; }
    
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }
}