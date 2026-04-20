namespace App.Core.Entities;

public class Contact : AuditableEntity
{
	public string FullName { get; set; } = default!;
	public string Message { get; set; } = default!;
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public EContactStatus Status { get; set; } = EContactStatus.New;
	public DateTimeOffset? ViewedAt { get; set; }
}
