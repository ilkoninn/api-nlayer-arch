namespace App.DAL.Persistence;

public static class AutomatedMigration
{
    public static async Task MigrateAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<AppDbContext>();

        if (context.Database.IsRelational())
            await context.Database.MigrateAsync();

        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await AppDbContextSeed.SeedDatabaseAsync(context, userManager, roleManager);
    }
}
