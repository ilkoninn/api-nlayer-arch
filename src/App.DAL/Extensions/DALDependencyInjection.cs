namespace App.DAL.Extensions;

public static class DALDependencyInjection
{
	public static IServiceCollection AddDAL(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDatabase(configuration);
		services.AddIdentity();
		services.AddRepositories();

		return services;
	}

	private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection");
		if (string.IsNullOrWhiteSpace(connectionString))
			throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing or empty.");

		services.AddDbContext<AppDbContext>(options =>
			options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)),
			mySqlOptions => mySqlOptions.EnableStringComparisonTranslations()));

		//options.UseSqlServer(connectionString));

		services.AddScoped<IDesignTimeDbContextFactory<AppDbContext>, AppDbContextFactory>();
	}

	private static void AddIdentity(this IServiceCollection services)
	{
		// Use AddIdentityCore instead of AddIdentity for APIs (no cookie authentication)
		services.AddIdentityCore<User>(options => options.SignIn.RequireConfirmedAccount = true)
			.AddRoles<IdentityRole<Guid>>()
			.AddEntityFrameworkStores<AppDbContext>()
			.AddDefaultTokenProviders();

		services.Configure<IdentityOptions>(options =>
		{
			options.Password.RequireDigit = true;
			options.Password.RequireLowercase = true;
			options.Password.RequireNonAlphanumeric = true;
			options.Password.RequireUppercase = true;
			options.Password.RequiredLength = 8;
			options.Password.RequiredUniqueChars = 1;

			options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
			options.Lockout.MaxFailedAccessAttempts = 5;
			options.Lockout.AllowedForNewUsers = true;

			options.User.AllowedUserNameCharacters =
				"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
			options.User.RequireUniqueEmail = true;
		});
	}

	private static void AddRepositories(this IServiceCollection services)
	{
		// Repositories
		services.AddScoped(
			typeof(IGenericRepository<,>),
			typeof(GenericRepository<,>)
		);

		services.AddScoped<IUnitOfWork, UnitOfWork>();
	}
}
