namespace App.Business.DTOs.TeamMember;

public class TeamMemberListQueryDto
{
    public string? Search { get; set; }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
