namespace App.Business.DTOs.Auth;

public class ProfileUserDto : BaseEntityDto
{
	public bool IsAuth { get; set; } 
	public string Role { get; set; } = "Moderator";
	public string FullName { get; set; } = default!;
	public string UserName { get; set; } = default!;
	public string Email { get; set; } = default!;
	public string PhoneNumber { get; set; } = default!;
}
