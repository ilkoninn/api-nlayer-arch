namespace App.Business.Extensions;

public static class BusinessDependencyInjection
{
	public static IServiceCollection AddBusiness(this IServiceCollection services)
	{
		services.AddServices();

		return services;
	}

	private static void AddServices(this IServiceCollection services)
	{
		// External Services
		services.AddHttpContextAccessor();
		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<IFileService, FileService>();
		services.AddScoped<IClaimService, ClaimService>();

		// Internal Services
		services.AddScoped<IFaqService, FaqService>();
		services.AddScoped<IUserService, UserService>();
		services.AddScoped<ISettingService, SettingService>();
		services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<ITeamMemberService, TeamMemberService>();

        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<IBlogCategoryService, BlogCategoryService>();

	}
}
