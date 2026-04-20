namespace App.Core.Entities;

public class TeamMember : AuditableEntity
{
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
}
