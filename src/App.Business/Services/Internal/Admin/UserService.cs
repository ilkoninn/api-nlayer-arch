namespace App.Business.Services.Internal;

public sealed class UserService(
	IClaimService claimService,
    UserManager<User> userManager,
	IUnitOfWork unitOfWork) : IUserService
{
	//  ===============================
	//  Admin operations
	//  ===============================

	public async Task<PagedResult<UserResponseDto>> GetAllAsync(UserListQueryDto query, CancellationToken ct = default)
	{
        var currentUserId = claimService.GetCurrentUserId();

        IQueryable<User> q = userManager.Users
            .Where(u => u.Id != Guid.Parse(currentUserId) && !u.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
		{
			var s = query.Search.Trim();

			q = q.Where(u =>
				(u.UserName != null && u.UserName.Contains(s)) ||
				(u.FullName != null && u.FullName.Contains(s)) ||
				(u.Email != null && u.Email.Contains(s))
			);
		}

		q = q.OrderBy(u => u.UserName);

		var pagedUsers = await q.ToPagedResultAsync(query.PageNumber, query.PageSize, ct);

		var dtoItems = new List<UserResponseDto>(pagedUsers.Items.Count);

		foreach (var u in pagedUsers.Items)
		{
			var roles = await userManager.GetRolesAsync(u);
			var roleName = roles.FirstOrDefault() ?? "Moderator";

			dtoItems.Add(new UserResponseDto
			{
				Id = u.Id,
				Email = u.Email ?? string.Empty,
				UserName = u.UserName ?? string.Empty,
				FullName = u.FullName ?? string.Empty,
				PhoneNumber = u.PhoneNumber,
				UserRole = roleName,
				IsBanned = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow
			});
		}

		return new PagedResult<UserResponseDto>
		{
			Items = dtoItems,
			PageNumber = pagedUsers.PageNumber,
			PageSize = pagedUsers.PageSize,
			TotalCount = pagedUsers.TotalCount
		};
	}

	public async Task<UserResponseDto> GetByIdAsync(Guid Id, CancellationToken ct = default)
	{
		var user = await userManager.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.Id == Id && !u.IsDeleted, ct);

		if (user is null)
			throw new KeyNotFoundException($"İstifadəçi tapılmadı: '{Id}'.");

		var role = (await userManager.GetRolesAsync(user)).FirstOrDefault();

		return new UserResponseDto
		{
			Id = user.Id,
			Email = user.Email ?? string.Empty,
			UserName = user.UserName ?? string.Empty,
			FullName = user.FullName ?? string.Empty,
			PhoneNumber = user.PhoneNumber,
			UserRole = role ?? "Moderator",
			IsBanned = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow
		};
	}

	public async Task CreateAsync(CreateUserDto createUserDTO, CancellationToken ct = default)
	{
		var existingByEmail = await userManager.FindByEmailAsync(createUserDTO.Email!);
		if (existingByEmail is not null)
			throw new InvalidOperationException("E-poçt artıq mövcuddur.");

		var existingByUserName = await userManager.FindByNameAsync(createUserDTO.UserName!);
		if (existingByUserName is not null)
			throw new InvalidOperationException("İstifadəçi adı artıq mövcuddur.");

		var user = new User
		{
			Email = createUserDTO.Email,
			UserName = createUserDTO.UserName,
			FullName = createUserDTO.FullName,
			PhoneNumber = createUserDTO.PhoneNumber
		};

		IdentityResult result;

		if (!string.IsNullOrWhiteSpace(createUserDTO.Password))
			result = await userManager.CreateAsync(user, createUserDTO.Password);
		else
			result = await userManager.CreateAsync(user, "Moderator123!@");

		if (!result.Succeeded)
		{
			var msg = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
			throw new InvalidOperationException($"İstifadəçi yaratmaq uğursuz oldu: {msg}");
		}

		// Assign role if provided
		if (!string.IsNullOrWhiteSpace(createUserDTO.Role))
		{
			var roleResult = await userManager.AddToRoleAsync(user, createUserDTO.Role);
			if (!roleResult.Succeeded)
			{
				var msg = string.Join("; ", roleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
				throw new InvalidOperationException($"İstifadəçi yaradıldı, lakin rol təyini uğursuz oldu: {msg}");
			}
		}

		await unitOfWork.SaveChangesAsync(ct);
	}

	public async Task UpdateAsync(Guid id, UpdateUserDto updateUserDTO, CancellationToken ct = default)
	{
		var user = await userManager.Users
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(u => u.Id == id, ct);

		if (user is null)
			throw new KeyNotFoundException($"İstifadəçi tapılmadı: '{id}'.");
		if (user.IsDeleted)
			throw new KeyNotFoundException($"İstifadəçi tapılmadı: '{id}'.");

		if (!string.IsNullOrWhiteSpace(updateUserDTO.UserName))
			user.UserName = updateUserDTO.UserName;

		if (!string.IsNullOrWhiteSpace(updateUserDTO.FullName))
			user.FullName = updateUserDTO.FullName;

		if (updateUserDTO.PhoneNumber is not null)
			user.PhoneNumber = updateUserDTO.PhoneNumber;

		var result = await userManager.UpdateAsync(user);
		if (!result.Succeeded)
		{
			var msg = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
			throw new InvalidOperationException($"İstifadəçi yeniləmək uğursuz oldu: {msg}");
		}

		// Update role if provided
		if (!string.IsNullOrWhiteSpace(updateUserDTO.Role))
		{
			var currentRoles = await userManager.GetRolesAsync(user);
			if (currentRoles.Any())
			{
				var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
				if (!removeResult.Succeeded)
				{
					var msg = string.Join("; ", removeResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
					throw new InvalidOperationException($"Mövcud rolları silmək uğursuz oldu: {msg}");
				}
			}

			var addResult = await userManager.AddToRoleAsync(user, updateUserDTO.Role);
			if (!addResult.Succeeded)
			{
				var msg = string.Join("; ", addResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
				throw new InvalidOperationException($"Yeni rol təyin etmək uğursuz oldu: {msg}");
			}
		}

		await unitOfWork.SaveChangesAsync(ct);
	}

	public async Task DeleteAsync(Guid id, CancellationToken ct = default)
	{
		var user = await userManager.Users
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(u => u.Id == id, ct);

		if (user is null)
			throw new KeyNotFoundException($"İstifadəçi tapılmadı: '{id}'.");
		if (user.IsDeleted)
			throw new KeyNotFoundException($"İstifadəçi tapılmadı: '{id}'.");

		var deletedUserName = BuildDeletedUserName();
		var deletedEmail = BuildDeletedEmail();

		user.UserName = deletedUserName;
		user.NormalizedUserName = deletedUserName.ToUpperInvariant();
		user.Email = deletedEmail;
		user.NormalizedEmail = deletedEmail.ToUpperInvariant();
		user.EmailForStudent = null;
		user.PhoneNumber = null;
		user.Phone = null;
		user.Slug = null;
		user.PasswordHash = null;
		user.IsDeleted = true;
		user.IsActive = false;
		user.LockoutEnd = null;
		user.LockoutEnabled = false;

		var result = await userManager.UpdateAsync(user);
		if (!result.Succeeded)
		{
			var msg = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
			throw new InvalidOperationException($"İstifadəçi silmək uğursuz oldu: {msg}");
		}

		await unitOfWork.SaveChangesAsync(ct);
	}

	private static string BuildDeletedUserName() => $"deleted-{Guid.NewGuid():N}";

	private static string BuildDeletedEmail() => $"deleted-{Guid.NewGuid():N}@deleted.local";

	public async Task BanAsync(Guid id, CancellationToken ct = default)
	{
		var user = await userManager.Users
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(u => u.Id == id, ct);

		if (user is null)
			throw new KeyNotFoundException($"İstifadəçi tapılmadı: '{id}'.");

		// banned
		if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
			throw new InvalidOperationException("İstifadəçi artıq bloklanmışdır.");

		// activate lockout
		var r1 = await userManager.SetLockoutEnabledAsync(user, true);
		if (!r1.Succeeded)
			throw new InvalidOperationException($"Bloklanmanı aktivləşdirmək uğursuz oldu: {string.Join("; ", r1.Errors.Select(e => $"{e.Code}: {e.Description}"))}");

		// timeless ban
		var r2 = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
		if (!r2.Succeeded)
			throw new InvalidOperationException($"Bloklanma tarixini təyin etmək uğursuz oldu: {string.Join("; ", r2.Errors.Select(e => $"{e.Code}: {e.Description}"))}");

		await unitOfWork.SaveChangesAsync(ct);
	}

	public async Task UnBanAsync(Guid id, CancellationToken ct = default)
	{
		var user = await userManager.Users
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(u => u.Id == id, ct);

		if (user is null)
			throw new KeyNotFoundException($"İstifadəçi tapılmadı: '{id}'.");

		if (!user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow)
			throw new InvalidOperationException("İstifadəçi bloklanmamışdır.");

		var r1 = await userManager.SetLockoutEndDateAsync(user, null);
		if (!r1.Succeeded)
			throw new InvalidOperationException($"Bloklanmanı ləğv etmək uğursuz oldu: {string.Join("; ", r1.Errors.Select(e => $"{e.Code}: {e.Description}"))}");

		var r2 = await userManager.ResetAccessFailedCountAsync(user);
		if (!r2.Succeeded)
			throw new InvalidOperationException($"Uğursuz giriş sayını sıfırlamaq uğursuz oldu: {string.Join("; ", r2.Errors.Select(e => $"{e.Code}: {e.Description}"))}");

		await unitOfWork.SaveChangesAsync(ct);
	}

	public async Task AssignRoleAsync(Guid userId, string roleName, CancellationToken ct = default)
	{
		if (string.IsNullOrWhiteSpace(roleName))
			throw new ArgumentException("Rol adı boş ola bilməz.", nameof(roleName));

		var user = await userManager.Users
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(u => u.Id == userId, ct);

		if (user is null)
			throw new KeyNotFoundException($"İstifadəçi tapılmadı: '{userId}'.");

		var currentRoles = await userManager.GetRolesAsync(user);
		if (currentRoles.Contains(roleName))
			throw new InvalidOperationException($"İstifadəçi artıq '{roleName}' roluna malikdir.");

		var result = await userManager.AddToRoleAsync(user, roleName);
		if (!result.Succeeded)
		{
			var msg = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
			throw new InvalidOperationException($"Rol təyin etmək uğursuz oldu: {msg}");
		}

		await unitOfWork.SaveChangesAsync(ct);
	}

	public async Task RemoveRoleAsync(Guid userId, string roleName, CancellationToken ct = default)
	{
		if (string.IsNullOrWhiteSpace(roleName))
			throw new ArgumentException("Rol adı boş ola bilməz.", nameof(roleName));

		var user = await userManager.Users
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(u => u.Id == userId, ct);

		if (user is null)
			throw new KeyNotFoundException($"İstifadəçi tapılmadı: '{userId}'.");

		var currentRoles = await userManager.GetRolesAsync(user);
		if (!currentRoles.Contains(roleName))
			throw new InvalidOperationException($"İstifadəçinin '{roleName}' rolu yoxdur.");

		var result = await userManager.RemoveFromRoleAsync(user, roleName);
		if (!result.Succeeded)
		{
			var msg = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
			throw new InvalidOperationException($"Rolu silmək uğursuz oldu: {msg}");
		}

		await unitOfWork.SaveChangesAsync(ct);
	}
}
