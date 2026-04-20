namespace App.Business.DTOs.Blog.Admin;

public class UpdateBlogDto : CreateBlogDto
{
    // Flags to delete images
    public bool DeleteCoverImage { get; set; }
    public bool DeleteOgImage { get; set; }
}
