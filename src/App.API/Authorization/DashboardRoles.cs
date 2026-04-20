namespace App.API.Authorization;

/// <summary>
/// Müəllim iş otağı API-ləri — yalnız Instructor. Admin öz şəxsi dərs məlumatı üçün ayrıca müəllim hesabı ilə daxil olur.
/// </summary>
public static class DashboardRoles
{
	public const string PlatformAdmin = nameof(App.Core.Enums.EUserRole.Admin);
}
