namespace App.Business.DTOs.TeamMember;

public class TeamMemberDto : BaseEntityDto
{
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string ProfileImageUrl { get; set; } = null!;
}
