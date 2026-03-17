namespace App.Business.DTOs.Contact.Admin;

public class ContactListQueryDto
{
	public string? Search { get; set; }
	public EContactStatus? Status { get; set; }

	public int PageNumber { get; set; } = 1;
	public int PageSize { get; set; } = 20;	

}
