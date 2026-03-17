namespace App.Business.DTOs.User.Admin;

public class CreateUserDto
{
	public string FullName { get; set; } = string.Empty;
	public string? UserName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? Password { get; set; }
	public string? Role { get; set; }
}
