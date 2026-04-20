namespace App.Business.Services.Internal;

public class SettingService(
	IGenericRepository<Setting, Guid> repository,
	IUnitOfWork unitOfWork,
	IFileService fileService) : ISettingService
{
	//  ===============================
	//  Public operations
	//  ===============================

	public async Task<PagedResult<SettingDto>> GetAllPublicAsync(SettingListQueryDto query, CancellationToken ct = default)
	{
        IQueryable<Setting> q = repository.GetAll();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            q = q.Where(s =>
                (s.Key != null && s.Key.Contains(search)) ||
                (s.Value != null && s.Value.Contains(search))
            );
        }

        q = q.OrderByDescending(s => s.CreatedOn);

        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var pagedSettings = await q.ToPagedResultAsync(pageNumber, pageSize, ct);

        var dtoItems = pagedSettings.Items.Select(s => new SettingDto
        {
            Id = s.Id,
            Key = s.Key,
            Value = s.Value,
            IsFile = !string.IsNullOrWhiteSpace(s.Value) && s.Value.Contains("/uploads/")
        }).ToList();

        return new PagedResult<SettingDto>
        {
            Items = dtoItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = pagedSettings.TotalCount
        };
    }

	//  ===============================
	//  Admin operations
	//  ===============================

	public async Task<bool> CreateAsync(CreateSettingDto createDto, CancellationToken ct = default)
	{
		if (createDto is null)
			throw new ArgumentNullException(nameof(createDto));

		var key = createDto.Key?.Trim();

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentException("Açar boş buraxıla bilməz.", nameof(createDto.Key));

		var existingKey = await repository.GetAsync(s => s.Key == key, null, ct);
		if (existingKey is not null)
			throw new InvalidOperationException($"Bu açar artıq mövcuddur: '{key}'.");

		var value = createDto.Value?.Trim();

		// If file provided, upload it and use URL as value
		if (createDto.File is not null)
		{
			value = await fileService.UploadAsync(createDto.File, "settings", ct);
		}
		else if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("Qiymət boş buraxıla bilməz.", nameof(createDto.Value));
		}

		var setting = new Setting
		{
			Key = key!,
			Value = value!
		};

		await repository.AddAsync(setting, ct);
		await unitOfWork.SaveChangesAsync(ct);

		return true;
	}

	public async Task<bool> UpdateAsync(Guid id, UpdateSettingDto updateDto, CancellationToken ct = default)
	{
		if (updateDto is null)
			throw new ArgumentNullException(nameof(updateDto));

		var key = updateDto.Key?.Trim();

		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentException("Açar boş buraxıla bilməz.", nameof(updateDto.Key));

		var entity = await repository.GetByIdAsync(id, null, ct)
			?? throw new KeyNotFoundException($"Tənzimləmə tapılmadı: '{id}'.");

		if (entity.Key != key)
		{
			var existingKey = await repository.GetAsync(s => s.Key == key && s.Id != id, null, ct);
			if (existingKey is not null)
				throw new InvalidOperationException($"Bu açar artıq mövcuddur: '{key}'.");
		}

		var value = updateDto.Value?.Trim();

		// Handle file deletion
		if (updateDto.DeleteFile)
		{
			// Delete file if it's a URL
			if (!string.IsNullOrWhiteSpace(entity.Value) && entity.Value.Contains("/uploads/"))
			{
				await fileService.DeleteAsync(entity.Value, ct);
			}
			
			// Set to empty string or provided text value
			value = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
		}
		else if (updateDto.File is not null)
		{
			// Replace with new file
			// Delete old file if it's a URL
			if (!string.IsNullOrWhiteSpace(entity.Value) && entity.Value.Contains("/uploads/"))
			{
				await fileService.DeleteAsync(entity.Value, ct);
			}

			value = await fileService.UploadAsync(updateDto.File, "settings", ct);
		}
		else if (string.IsNullOrWhiteSpace(value))
		{
			// If no file, no delete flag, and no value provided, keep existing
			value = entity.Value;
		}

		entity.Key = key!;
		entity.Value = value!;

		repository.Update(entity);
		await unitOfWork.SaveChangesAsync(ct);

		return true;
	}

	public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
	{
		var entity = await repository.GetByIdAsync(id, null, ct)
			?? throw new KeyNotFoundException($"Tənzimləmə tapılmadı: '{id}'.");

		// Delete file if value is URL
		if (!string.IsNullOrWhiteSpace(entity.Value) && 
			(entity.Value.StartsWith("http") || entity.Value.Contains("/uploads/")))
		{
			await fileService.DeleteAsync(entity.Value, ct);
		}

		repository.Remove(entity);
		await unitOfWork.SaveChangesAsync(ct);

		return true;
	}
}
