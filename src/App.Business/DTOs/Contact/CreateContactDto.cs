namespace App.Business.DTOs.Contact;

public class CreateContactDto
{
	public string FullName { get; set; } = null!;
	public string Message { get; set; } = null!;
	public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}
