namespace App.DAL.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var basePath = ResolveConfigurationDirectory();

		var config = new ConfigurationBuilder()
			.SetBasePath(basePath)
			.AddJsonFile("appsettings.json", optional: false)
			.AddJsonFile("appsettings.Development.json", optional: true)
			.AddEnvironmentVariables()
			.Build();

		var connectionString = config.GetConnectionString("DefaultConnection");

		var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

		optionsBuilder.UseMySql(connectionString,
			new MySqlServerVersion(new Version(8, 0, 21)),
			mySqlOptions => mySqlOptions.EnableStringComparisonTranslations());

		return new AppDbContext(optionsBuilder.Options);
	}

	/// <summary>
	/// dotnet ef işə salınanda CWD çox vaxt App.DAL olur; appsettings isə App.API-dədir.
	/// </summary>
	private static string ResolveConfigurationDirectory()
	{
		var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
		while (dir != null)
		{
			if (dir.Name.Equals("App.API", StringComparison.OrdinalIgnoreCase))
			{
				var here = Path.Combine(dir.FullName, "appsettings.json");
				if (File.Exists(here))
					return dir.FullName;
			}

			var apiSettings = Path.Combine(dir.FullName, "App.API", "appsettings.json");
			if (File.Exists(apiSettings))
				return Path.Combine(dir.FullName, "App.API");

			dir = dir.Parent;
		}

		throw new InvalidOperationException(
			"appsettings.json tapılmadı. App.API qovluğunda fayl olmalıdır. " +
			"Əmri back/src/App.DAL və ya back/src/App.API-dən işlədin, və ya --project ilə startup layihəsini göstərin.");
	}
}
