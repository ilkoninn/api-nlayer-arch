namespace App.Business.Services.External;

public class AuthService(
	UserManager<User> manager,
	IConfiguration configuration,
	IClaimService claimService) : IAuthService

{
	//  ==============================
	//  Public operations
	//  ==============================

	public async Task<string> GetAccessTokenAsync(LoginDto loginDto)
	{
		var key = loginDto.Username.Trim();
		var user = await manager.FindByNameAsync(key)
			?? await manager.FindByEmailAsync(key);
		if (user is null)
			throw new UnauthorizedAccessException("Daxilolma uğursuz.");

		if (user.IsDeleted)
			throw new UnauthorizedAccessException("Hesab tapılmadı.");

		if (user.LockoutEnd > DateTimeOffset.Now)
			throw new InvalidOperationException("İstifadəçi Admin tərəfindən bloklanıb, Adminlə əlaqə saxlayın.");

		if (!await manager.CheckPasswordAsync(user, loginDto.Password))
			throw new UnauthorizedAccessException("Şifrə yanlışdır.");

		var roles = await manager.GetRolesAsync(user);
		if (roles.Count == 0)
			throw new UnauthorizedAccessException("Hesaba rol təyin edilməyib.");

		var token = JwtTokenGenerator.GenerateToken(user.Id, roles, configuration);

		return token;
	}

	//  ==============================
	//  Admin operations
	//  ==============================

	public async Task<ProfileUserDto> GetMySummaryAsync(CancellationToken ct = default)
	{
		var userId = claimService.GetCurrentUserId() ?? throw new Exception("User ID not found in claims.");

		if (string.IsNullOrWhiteSpace(userId))
			return new ProfileUserDto { IsAuth = false };

		var user = await manager.Users.AsNoTracking()
									  .Where(u => u.Id.ToString() == userId)
									  .FirstOrDefaultAsync(ct);

		if (user is null)
			return new ProfileUserDto { IsAuth = false };

		if (user.IsDeleted)
			return new ProfileUserDto { IsAuth = false };

		var roleList = (await manager.GetRolesAsync(user)).ToList();
		var displayRole = PickDisplayRole(roleList);

		var jwtToken = claimService.GetCurrentUserJwtToken() ?? string.Empty;

		var expiration = JwtTokenGenerator.GetTokenExpiration(jwtToken, configuration);

		return new ProfileUserDto
		{
			Id = user.Id,
			IsAuth = expiration > DateTime.UtcNow,
			FullName = user.FullName ?? string.Empty,
			Email = user.Email ?? string.Empty,
			UserName = user.UserName ?? string.Empty,
			PhoneNumber = user.PhoneNumber ?? string.Empty,
			PayoutCardHint = user.PayoutCardHint,
			Role = displayRole,
			Roles = roleList,
		};
	}

	public async Task<bool> UpdateProfileAsync(UpdateProfileUserDto updateProfileUserDto, CancellationToken ct = default)
	{
		var userId = claimService.GetCurrentUserId() ?? throw new Exception("User ID not found in claims.");

		if (string.IsNullOrWhiteSpace(userId))
			return false;

		var user = await manager.FindByIdAsync(userId);

		if (user is null) return false;

		user.FullName = updateProfileUserDto.FullName?.Trim() ?? string.Empty;
		user.UserName = updateProfileUserDto.UserName?.Trim() ?? user.UserName;
		user.Email = updateProfileUserDto.Email?.Trim() ?? user.Email;
		user.PhoneNumber = updateProfileUserDto.PhoneNumber?.Trim() ?? string.Empty;

		var rolesForProfile = await manager.GetRolesAsync(user);

		var result = await manager.UpdateAsync(user);

		if (!result.Succeeded)
			throw new Exception("Failed to update profile.");

		if (!string.IsNullOrWhiteSpace(updateProfileUserDto.Password))
		{
			var resetToken = await manager.GeneratePasswordResetTokenAsync(user);
			var passwordResult = await manager.ResetPasswordAsync(user, resetToken, updateProfileUserDto.Password);

			if (!passwordResult.Succeeded)
				throw new Exception("Failed to update password.");
		}

		return true;
	}

	private static string PickDisplayRole(IReadOnlyList<string> roles)
	{
		if (roles.Contains(EUserRole.Admin.ToString())) return EUserRole.Admin.ToString();
		return roles.Count > 0 ? roles[0] : string.Empty;
	}
}
