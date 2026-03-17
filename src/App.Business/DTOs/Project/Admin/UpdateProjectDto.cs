namespace App.Business.DTOs.Project.Admin;

public class UpdateProjectDto
{
    public string Title { get; set; } = null!;
    public string Excerpt { get; set; } = null!;
    public string Content { get; set; } = null!;
    public Guid ProjectCategoryId { get; set; }
    public List<IFormFile>? Images { get; set; }
    
    // JSON string from form, will be deserialized in controller
    public string? FeaturesJson { get; set; }

    // Populated after deserialization
    [SwaggerExclude]
    [JsonIgnore]
    [BindNever]
    public List<ProjectFeatureInputDto>? Features { get; set; }
    
    // Image IDs to remove
    public List<Guid>? RemoveImageIds { get; set; }
    
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }
}