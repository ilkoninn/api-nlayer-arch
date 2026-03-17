namespace App.Business.DTOs.TeamMember.Admin;

public class UpdateTeamMemberDto : CreateTeamMemberDto
{
    // Flag to delete profile image
    public bool DeleteProfileImage { get; set; }
}
