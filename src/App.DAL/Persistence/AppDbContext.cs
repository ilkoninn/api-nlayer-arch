namespace App.DAL.Persistence;

public class AppDbContext(
	DbContextOptions<AppDbContext> options,
	IClaimService? claimService = null) : IdentityDbContext<User, IdentityRole<Guid>,
		Guid>(options)
{
	// Models
	public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
	public DbSet<Service> Services => Set<Service>();
    public DbSet<Setting> Settings => Set<Setting>();
	public DbSet<Contact> Contacts => Set<Contact>();
	public DbSet<Faq> Faqs => Set<Faq>();

	// Blogs
	public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
	public DbSet<Blog> Blogs => Set<Blog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}

	public new async Task<int> SaveChangesAsync(CancellationToken ct = new())
	{
		var userId = claimService?.GetCurrentUserId();

		foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
		{
			switch (entry.State)
			{
				case EntityState.Added:
					entry.Entity.CreatedById = userId;
					entry.Entity.CreatedOn = DateTime.UtcNow;

					entry.Entity.LastModifiedById = userId;
					entry.Entity.LastModifiedOn = DateTime.UtcNow;
					break;

				case EntityState.Modified:
					entry.Entity.LastModifiedById = userId;
					entry.Entity.LastModifiedOn = DateTime.UtcNow;
					break;
			}
		}

		foreach (var entry in ChangeTracker.Entries<User>())
		{
			switch (entry.State)
			{
				case EntityState.Added:
					entry.Entity.CreatedById = userId;
					entry.Entity.CreatedOn = DateTimeOffset.UtcNow;
					entry.Entity.LastModifiedById = userId;
					entry.Entity.LastModifiedOn = DateTimeOffset.UtcNow;
					break;
				case EntityState.Modified:
					entry.Entity.LastModifiedById = userId;
					entry.Entity.LastModifiedOn = DateTimeOffset.UtcNow;
					break;
			}
		}

		return await base.SaveChangesAsync(ct);
	}
}
