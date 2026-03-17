namespace App.DAL.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var basePath = Directory.GetCurrentDirectory();

		var config = new ConfigurationBuilder()
			.SetBasePath(basePath)
			.AddJsonFile("appsettings.json", optional: false)
			.AddJsonFile($"appsettings.Development.json", optional: true)
			.AddEnvironmentVariables()
			.Build();

		var connectionString = config.GetConnectionString("DefaultConnection");

		var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

		//optionsBuilder.UseSqlServer(connectionString);
		optionsBuilder.UseMySql(connectionString,
			new MySqlServerVersion(new Version(8, 0, 21)),
		mySqlOptions => mySqlOptions.EnableStringComparisonTranslations());

		return new AppDbContext(optionsBuilder.Options);
	}
}
