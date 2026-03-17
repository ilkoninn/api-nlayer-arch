namespace App.Business.DTOs.Project;

public class ProjectFeatureDto : BaseEntityDto
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}