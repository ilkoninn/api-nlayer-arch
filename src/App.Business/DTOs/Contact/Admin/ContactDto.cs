using System.Text.Json.Serialization;

namespace App.Business.DTOs.Contact.Admin;

public class ContactDto : AuditableEntityDto
{
	public string FullName { get; set; } = default!;
	public string Message { get; set; } = default!;
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }

	[JsonConverter(typeof(JsonStringEnumConverter))]
	public EContactStatus ContactStatus { get; set; }
	public DateTimeOffset? ViewedAt { get; set; }
}
