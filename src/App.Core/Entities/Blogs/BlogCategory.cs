namespace App.Core.Entities.Blogs;

public class BlogCategory : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;

    // Relations
    public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
