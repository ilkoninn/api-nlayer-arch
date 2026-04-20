namespace App.Business.DTOs.Auth;

public class ProfileUserDto : BaseEntityDto
{
	public bool IsAuth { get; set; } 
	public string Role { get; set; } = "Student";
	public List<string> Roles { get; set; } = [];
	public string FullName { get; set; } = default!;
	public string UserName { get; set; } = default!;
	public string Email { get; set; } = default!;
	public string PhoneNumber { get; set; } = default!;

	/// <summary>Müəllim üçün köçürmə kartı / hesab izahı (tələbə ödənişi üçün).</summary>
	public string? PayoutCardHint { get; set; }
}
