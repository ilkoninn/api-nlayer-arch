namespace App.DAL.Persistence;

public static class AppDbContextSeed
{
    public static async Task SeedDatabaseAsync(
        AppDbContext context, 
        UserManager<User> userManager, 
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var role in Enum.GetValues<EUserRole>().Select(x => x.ToString()))
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new(role));
            }
        }

        var adminExists = await userManager.FindByNameAsync("admin");

        if (adminExists == null)
        {
            var userAdmin = new User { UserName = "admin", Email = "admin@admin.com", EmailConfirmed = true };
            await userManager.CreateAsync(userAdmin, "Admin123!@");
            await userManager.AddToRoleAsync(userAdmin, EUserRole.Admin.ToString());
        }

        await context.SaveChangesAsync();
    }
}
