namespace App.Business.DTOs.User.Admin;

public class UserResponseDto : BaseEntityDto
{
	public string FullName { get; set; } = string.Empty;
	public string? UserName { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string UserRole { get; set; } = string.Empty;
	public bool IsBanned { get; set; }
}
