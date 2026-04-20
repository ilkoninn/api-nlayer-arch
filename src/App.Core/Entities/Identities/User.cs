namespace App.Core.Entities.Identities;

public class User : IdentityUser<Guid>, IRepositoryEntity<Guid>
{
	public string? FullName { get; set; }

	/// <summary>Müəllim üçün: tələbələrin köçürmə edəcəyi kart / hesab izahı (maskalanmış PAN və s.).</summary>
	public string? PayoutCardHint { get; set; }

	// Per-student payment schedule (only meaningful when role == Student).
	/// <summary>Tələbə üçün: hər ayın neçəsinde ödəniş edilməlidir (1-28).</summary>
	public int? MonthlyDueDay { get; set; }
	/// <summary>Tələbə üçün: ödənişdə icazəli gecikmi günü (0 = yoxdur).</summary>
	public int? PaymentGraceDays { get; set; }

	// Shared soft-delete/audit style fields used by dashboard.
	public DateTimeOffset CreatedOn { get; set; }
	public string? CreatedById { get; set; }
	public DateTimeOffset LastModifiedOn { get; set; }
	public string? LastModifiedById { get; set; }
	public bool IsDeleted { get; set; }
	public bool IsActive { get; set; } = true;

	// Student profile fields (only meaningful when role == Student).
	public Guid? TutorId { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? ParentName { get; set; }
	public string? SchoolClassOrProgram { get; set; }
	public string? PreparationTrack { get; set; }
	public string? Phone { get; set; }
	public string? EmailForStudent { get; set; }
	public string? InternalNotes { get; set; }
	public string? Slug { get; set; }

}
