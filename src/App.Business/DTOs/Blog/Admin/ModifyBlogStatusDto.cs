namespace App.Business.DTOs.Blog.Admin;

public class ModifyBlogStatusDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EBlogStatus Status { get; set; }
}