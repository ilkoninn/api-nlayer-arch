namespace App.Business.DTOs.TeamMember.Admin;

public class CreateTeamMemberDto
{
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public IFormFile? ProfileImage { get; set; }
}
