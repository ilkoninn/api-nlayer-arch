namespace App.Business.Services.Internal;

public class ServiceService(
    IGenericRepository<Service, Guid> repository,
    IUnitOfWork unitOfWork,
    IFileService fileService) : IServiceService
{
    //  ===============================
    //  Public operations
    //  ===============================

    public async Task<PagedResult<ServiceAllDto>> GetAllPublicAsync(ServiceListQueryDto qDto, CancellationToken ct = default)
    {
        IQueryable<Service> query = repository.GetAll();

        if (!string.IsNullOrWhiteSpace(qDto.Search))
        {
            var search = qDto.Search.Trim();

            query = query.Where(s =>
                (s.Title != null && s.Title.Contains(search)) ||
                (s.Excerpt != null && s.Excerpt.Contains(search))
            );
        }

        query = query.OrderByDescending(s => s.CreatedOn);

        if (qDto.PageNumber <= 0 && qDto.PageSize <= 0)
        {
            var allServices = await query.ToListAsync(ct);

            var allDtoItems = allServices.Select(MapToServiceAllDto).ToList();

            return new PagedResult<ServiceAllDto>
            {
                Items = allDtoItems,
                PageNumber = 1,
                PageSize = allDtoItems.Count,
                TotalCount = allDtoItems.Count
            };
        }

        var pageNumber = qDto.PageNumber <= 0 ? 1 : qDto.PageNumber;
        var pageSize = qDto.PageSize <= 0 ? 20 : qDto.PageSize;

        var pagedServices = await query.ToPagedResultAsync(pageNumber, pageSize, ct);

        var dtoItems = pagedServices.Items.Select(MapToServiceAllDto).ToList();

        return new PagedResult<ServiceAllDto>
        {
            Items = dtoItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = pagedServices.TotalCount
        };
    }

    public async Task<ServiceDto> GetDetailBySlugAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug boş ola bilməz.", nameof(slug));

        var entity = await repository.GetAsync(s => s.Slug == slug, null, ct)
            ?? throw new KeyNotFoundException($"Xidmət tapılmadı.");

        return MapToServiceDto(entity);
    }

    //  ===============================
    //  Admin operations
    //  ===============================

    public async Task<PagedResult<ServiceDto>> GetAllAsync(ServiceListQueryDto qDto, CancellationToken ct = default)
    {
        IQueryable<Service> query = repository.GetAll();

        if (!string.IsNullOrWhiteSpace(qDto.Search))
        {
            var search = qDto.Search.Trim();

            query = query.Where(s =>
                (s.Title != null && s.Title.Contains(search)) ||
                (s.Excerpt != null && s.Excerpt.Contains(search))
            );
        }

        query = query.OrderByDescending(s => s.CreatedOn);

        var pageNumber = qDto.PageNumber <= 0 ? 1 : qDto.PageNumber;
        var pageSize = qDto.PageSize <= 0 ? 20 : qDto.PageSize;

        var pagedServices = await query.ToPagedResultAsync(pageNumber, pageSize, ct);

        var dtoItems = pagedServices.Items.Select(MapToServiceDto).ToList();

        return new PagedResult<ServiceDto>
        {
            Items = dtoItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = pagedServices.TotalCount
        };
    }

    public async Task<bool> CreateAsync(CreateServiceDto createDto, CancellationToken ct = default)
    {
        if (createDto is null)
            throw new ArgumentNullException(nameof(createDto));

        var title = createDto.Title?.Trim();
        var excerpt = createDto.Excerpt?.Trim();
        var content = createDto.Content?.Trim();

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Başlıq boş buraxıla bilməz.", nameof(createDto.Title));

        if (string.IsNullOrWhiteSpace(excerpt))
            throw new ArgumentException("Qısa məzmun boş buraxıla bilməz.", nameof(createDto.Excerpt));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Məzmun boş buraxıla bilməz.", nameof(createDto.Content));

        if (createDto.CoverImage is null)
            throw new ArgumentException("Şəkil boş buraxıla bilməz.", nameof(createDto.CoverImage));

        if (createDto.Icon is null)
            throw new ArgumentException("İkon boş buraxıla bilməz.", nameof(createDto.Icon));

        var slug = SlugGenerator.GenerateSlug(title);

        var existingSlug = await repository.GetAsync(s => s.Slug == slug, null, ct);
        if (existingSlug is not null)
            throw new InvalidOperationException($"Bu başlıqla xidmət artıq mövcuddur: '{title}'.");

        var coverImageUrl = await fileService.UploadAsync(createDto.CoverImage, "services", ct);
        var iconUrl = await fileService.UploadAsync(createDto.Icon, "services/icons", ct);

        var entity = new Service
        {
            Title = title,
            Slug = slug,
            Excerpt = excerpt,
            Content = content,
            CoverImageUrl = coverImageUrl,
            IconUrl = iconUrl,
            MetaTitle = createDto.MetaTitle?.Trim(),
            MetaDescription = createDto.MetaDescription?.Trim(),
            CanonicalUrl = createDto.CanonicalUrl?.Trim()
        };

        await repository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateServiceDto updateDto, CancellationToken ct = default)
    {
        if (updateDto is null)
            throw new ArgumentNullException(nameof(updateDto));

        var title = updateDto.Title?.Trim();
        var excerpt = updateDto.Excerpt?.Trim();
        var content = updateDto.Content?.Trim();

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Başlıq boş buraxıla bilməz.", nameof(updateDto.Title));

        if (string.IsNullOrWhiteSpace(excerpt))
            throw new ArgumentException("Qısa məzmun boş buraxıla bilməz.", nameof(updateDto.Excerpt));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Məzmun boş buraxıla bilməz.", nameof(updateDto.Content));

        var entity = await repository.GetByIdAsync(id, null, ct)
            ?? throw new KeyNotFoundException($"Xidmət tapılmadı: '{id}'.");

        var slug = SlugGenerator.GenerateSlug(title);

        if (entity.Slug != slug)
        {
            var existingSlug = await repository.GetAsync(s => s.Slug == slug && s.Id != id, null, ct);
            if (existingSlug is not null)
                throw new InvalidOperationException($"Bu başlıqla xidmət artıq mövcuddur: '{title}'.");
        }

        entity.Title = title;
        entity.Slug = slug;
        entity.Excerpt = excerpt;
        entity.Content = content;
        entity.MetaTitle = updateDto.MetaTitle?.Trim();
        entity.MetaDescription = updateDto.MetaDescription?.Trim();
        entity.CanonicalUrl = updateDto.CanonicalUrl?.Trim();

        // Handle cover image
        if (updateDto.DeleteCoverImage)
        {
            // Delete cover image
            if (!string.IsNullOrWhiteSpace(entity.CoverImageUrl))
            {
                await fileService.DeleteAsync(entity.CoverImageUrl, ct);
                entity.CoverImageUrl = null;
            }
        }
        else if (updateDto.CoverImage is not null)
        {
            // Replace cover image
            var oldCoverUrl = entity.CoverImageUrl;
            var newCoverUrl = await fileService.UploadAsync(updateDto.CoverImage, "services", ct);
            entity.CoverImageUrl = newCoverUrl;

            if (!string.IsNullOrWhiteSpace(oldCoverUrl))
            {
                await fileService.DeleteAsync(oldCoverUrl, ct);
            }
        }

        // Handle icon
        if (updateDto.DeleteIcon)
        {
            // Delete icon
            if (!string.IsNullOrWhiteSpace(entity.IconUrl))
            {
                await fileService.DeleteAsync(entity.IconUrl, ct);
                entity.IconUrl = null;
            }
        }
        else if (updateDto.Icon is not null)
        {
            // Replace icon
            var oldIconUrl = entity.IconUrl;
            var newIconUrl = await fileService.UploadAsync(updateDto.Icon, "services/icons", ct);
            entity.IconUrl = newIconUrl;

            if (!string.IsNullOrWhiteSpace(oldIconUrl))
            {
                await fileService.DeleteAsync(oldIconUrl, ct);
            }
        }

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, null, ct)
            ?? throw new KeyNotFoundException($"Xidmət tapılmadı: '{id}'.");

        if (!string.IsNullOrWhiteSpace(entity.CoverImageUrl))
        {
            await fileService.DeleteAsync(entity.CoverImageUrl, ct);
        }

        if (!string.IsNullOrWhiteSpace(entity.IconUrl))
        {
            await fileService.DeleteAsync(entity.IconUrl, ct);
        }

        repository.Remove(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    //  ===============================
    //  Private mapping
    //  ===============================

    private static ServiceAllDto MapToServiceAllDto(Service entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Slug = entity.Slug,
        Excerpt = entity.Excerpt,
        CoverImageUrl = entity.CoverImageUrl ?? string.Empty,
        IconUrl = entity.IconUrl ?? string.Empty
    };

    private static ServiceDto MapToServiceDto(Service entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Slug = entity.Slug,
        Excerpt = entity.Excerpt,
        Content = entity.Content,
        CoverImageUrl = entity.CoverImageUrl ?? string.Empty,
        IconUrl = entity.IconUrl ?? string.Empty,
        MetaTitle = entity.MetaTitle,
        MetaDescription = entity.MetaDescription,
        CanonicalUrl = entity.CanonicalUrl
    };
}