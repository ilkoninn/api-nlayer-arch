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
		var user = await manager.Users.FirstOrDefaultAsync(u => u.UserName == loginDto.Username) ??
			throw new KeyNotFoundException("İstifadəçi tapılmadı.");

		if (user.LockoutEnd > DateTimeOffset.Now)
			throw new InvalidOperationException("İstifadəçi Admin tərəfindən bloklanıb, Adminlə əlaqə saxlayın.");

		if (!await manager.CheckPasswordAsync(user, loginDto.Password))
			throw new UnauthorizedAccessException("Şifrə yanlışdır.");

		var role = (await manager.GetRolesAsync(user)).FirstOrDefault() ?? "InvalidRole";

		var token = JwtTokenGenerator.GenerateToken(user.Id, role, configuration);

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

		var role = (await manager.GetRolesAsync(user)).FirstOrDefault() ?? "Moderator";

		var jwtToken = claimService.GetCurrentUserJwtToken() ?? string.Empty;

		var expiration = JwtTokenGenerator.GetTokenExpiration(jwtToken, configuration);

		return new ProfileUserDto
		{
			Id = user.Id,
			IsAuth = expiration > DateTime.UtcNow,
			Email = user.Email ?? string.Empty,
			UserName = user.UserName ?? string.Empty,
			PhoneNumber = user.PhoneNumber ?? string.Empty,
			Role = role,
		};
	}

	public async Task<bool> UpdateProfileAsync(UpdateProfileUserDto updateProfileUserDto, CancellationToken ct = default)
	{
		var userId = claimService.GetCurrentUserId() ?? throw new Exception("User ID not found in claims.");

		if (string.IsNullOrWhiteSpace(userId))
			return false;

		var user = await manager.Users.AsNoTracking()
									  .Where(u => u.Id.ToString() == userId)
									  .FirstOrDefaultAsync(ct);

		if (user is null) return false;

		user.FullName = updateProfileUserDto.FullName;
		user.UserName = updateProfileUserDto.UserName;
		user.Email = updateProfileUserDto.Email;
		user.PhoneNumber = updateProfileUserDto.PhoneNumber;

		var result = await manager.UpdateAsync(user);

		if (!result.Succeeded)
			throw new Exception("Failed to update profile.");

		if (!string.IsNullOrWhiteSpace(updateProfileUserDto.Password))
		{
			var token = await manager.GeneratePasswordResetTokenAsync(user);
			var passwordResult = await manager.ResetPasswordAsync(user, token, updateProfileUserDto.Password);

			if (!passwordResult.Succeeded)
				throw new Exception("Failed to update password.");
		}

		return true;
	}
}
