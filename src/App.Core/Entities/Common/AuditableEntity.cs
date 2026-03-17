namespace App.Core.Entities.Common;

public abstract class AuditableEntity : BaseEntity
{
	public DateTimeOffset CreatedOn { get; set; }
	public string? CreatedById { get; set; }

	public DateTimeOffset LastModifiedOn { get; set; }
	public string? LastModifiedById { get; set; }

	public bool IsDeleted { get; set; } = false;
	public bool IsActive { get; set; } = true;
}
