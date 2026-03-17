namespace App.Business.Services.Internal;

public class TeamMemberService(
    IFileService fileService,
    IGenericRepository<TeamMember, Guid> repository,
    IUnitOfWork unitOfWork) : ITeamMemberService
{
    //  ===============================
    //  Public operations
    //  ===============================

    public async Task<PagedResult<TeamMemberDto>> GetAllAsync(TeamMemberListQueryDto query, CancellationToken ct = default)
    {
        IQueryable<TeamMember> q = repository.GetAll();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            q = q.Where(t =>
                (t.FullName != null && t.FullName.Contains(search)) ||
                (t.Role != null && t.Role.Contains(search))
            );
        }

        q = q.OrderByDescending(t => t.CreatedOn);

        if (query.PageNumber <= 0 && query.PageSize <= 0)
        {
            var allMembers = await q.ToListAsync(ct);

            var allDtoItems = allMembers.Select(MapToDto).ToList();

            return new PagedResult<TeamMemberDto>
            {
                Items = allDtoItems,
                PageNumber = 1,
                PageSize = allDtoItems.Count,
                TotalCount = allDtoItems.Count
            };
        }

        var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var pagedMembers = await q.ToPagedResultAsync(pageNumber, pageSize, ct);

        var dtoItems = pagedMembers.Items.Select(MapToDto).ToList();

        return new PagedResult<TeamMemberDto>
        {
            Items = dtoItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = pagedMembers.TotalCount
        };
    }

    //  ===============================
    //  Admin operations
    //  ===============================

    public async Task<bool> CreateAsync(CreateTeamMemberDto createDto, CancellationToken ct = default)
    {
        if (createDto is null)
            throw new ArgumentNullException(nameof(createDto));

        var fullName = createDto.FullName?.Trim();
        var role = createDto.Role?.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Ad və soyad boş buraxıla bilməz.", nameof(createDto.FullName));

        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Vəzifə boş buraxıla bilməz.", nameof(createDto.Role));

        if (createDto.ProfileImage is null)
            throw new ArgumentException("Profil şəkli boş buraxıla bilməz.", nameof(createDto.ProfileImage));

        var profileImageUrl = await fileService.UploadAsync(createDto.ProfileImage, "team", ct);

        var entity = new TeamMember
        {
            FullName = fullName,
            Role = role,
            ProfileImageUrl = profileImageUrl
        };

        await repository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateTeamMemberDto updateDto, CancellationToken ct = default)
    {
        if (updateDto is null)
            throw new ArgumentNullException(nameof(updateDto));

        var fullName = updateDto.FullName?.Trim();
        var role = updateDto.Role?.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Ad və soyad boş buraxıla bilməz.", nameof(updateDto.FullName));

        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Vəzifə boş buraxıla bilməz.", nameof(updateDto.Role));

        var entity = await repository.GetByIdAsync(id, null, ct)
            ?? throw new KeyNotFoundException($"Komanda üzvü tapılmadı: '{id}'.");

        entity.FullName = fullName;
        entity.Role = role;

        // Handle profile image
        if (updateDto.DeleteProfileImage)
        {
            // Delete profile image
            if (!string.IsNullOrWhiteSpace(entity.ProfileImageUrl))
            {
                await fileService.DeleteAsync(entity.ProfileImageUrl, ct);
                entity.ProfileImageUrl = null;
            }
        }
        else if (updateDto.ProfileImage is not null)
        {
            // Replace profile image
            var oldImageUrl = entity.ProfileImageUrl;
            var newImageUrl = await fileService.UploadAsync(updateDto.ProfileImage, "team", ct);
            entity.ProfileImageUrl = newImageUrl;

            if (!string.IsNullOrWhiteSpace(oldImageUrl))
            {
                await fileService.DeleteAsync(oldImageUrl, ct);
            }
        }

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, null, ct)
            ?? throw new KeyNotFoundException($"Komanda üzvü tapılmadı: '{id}'.");

        if (!string.IsNullOrWhiteSpace(entity.ProfileImageUrl))
        {
            await fileService.DeleteAsync(entity.ProfileImageUrl, ct);
        }

        repository.Remove(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    //  ===============================
    //  Private mapping
    //  ===============================

    private static TeamMemberDto MapToDto(TeamMember entity) => new()
    {
        Id = entity.Id,
        FullName = entity.FullName,
        Role = entity.Role,
        ProfileImageUrl = entity.ProfileImageUrl ?? string.Empty
    };
}
