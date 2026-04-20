
namespace App.DAL.Persistence.Configurations.Identities;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.Property(x => x.PayoutCardHint).HasMaxLength(500);
		builder.Property(x => x.FirstName).HasMaxLength(120);
		builder.Property(x => x.LastName).HasMaxLength(120);
		builder.Property(x => x.ParentName).HasMaxLength(200);
		builder.Property(x => x.SchoolClassOrProgram).HasMaxLength(200);
		builder.Property(x => x.PreparationTrack).HasMaxLength(120);
		builder.Property(x => x.InternalNotes).HasMaxLength(2000);
		builder.Property(x => x.Slug).HasMaxLength(256);
		builder.HasIndex(x => new { x.TutorId, x.Slug }).IsUnique();
		builder.HasIndex(x => new { x.TutorId, x.IsDeleted });
		builder.HasIndex(x => new { x.TutorId, x.CreatedOn });
	}
}
