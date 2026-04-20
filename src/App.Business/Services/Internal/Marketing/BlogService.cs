namespace App.Business.Services.Internal;

public class BlogService(
    IGenericRepository<Blog, Guid> repository,
    IGenericRepository<BlogCategory, Guid> categoryRepository,
    IUnitOfWork unitOfWork,
    IFileService fileService) : IBlogService
{
    //  ===============================
    //  Public operations
    //  ===============================

    public async Task<PagedResult<BlogAllDto>> GetAllPublicAsync(BlogListQueryDto qDto, CancellationToken ct = default)
    {
        IQueryable<Blog> query = repository.GetWhere(
            b => b.Status == EBlogStatus.Published,
            q => q.Include(b => b.BlogCategories));

        if (!string.IsNullOrWhiteSpace(qDto.Search))
        {
            var search = qDto.Search.Trim();

            query = query.Where(u =>
                (u.Title != null && u.Title.Contains(search))
            );
        }

        // Filter by category
        if (qDto.CategoryId.HasValue)
        {
            query = query.Where(b => b.BlogCategories.Any(bc => bc.Id == qDto.CategoryId.Value));
        }

        query = query.OrderByDescending(c => c.CreatedOn);

        if (qDto.PageNumber <= 0 && qDto.PageSize <= 0)
        {
            var allBlogs = await query.ToListAsync(ct);

            var allDtoItems = allBlogs.Select(MapToBlogAllDto).ToList();

            return new PagedResult<BlogAllDto>
            {
                Items = allDtoItems,
                PageNumber = 1,
                PageSize = allDtoItems.Count,
                TotalCount = allDtoItems.Count
            };
        }

        var pageNumber = qDto.PageNumber <= 0 ? 1 : qDto.PageNumber;
        var pageSize = qDto.PageSize <= 0 ? 20 : qDto.PageSize;

        var pagedBlogs = await query.ToPagedResultAsync(pageNumber, pageSize, ct);

        var dtoItems = pagedBlogs.Items.Select(MapToBlogAllDto).ToList();

        return new PagedResult<BlogAllDto>
        {
            Items = dtoItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = pagedBlogs.TotalCount
        };
    }

    public async Task<BlogDto> GetDetailBySlugAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug boş ola bilməz.", nameof(slug));

        var entity = await repository.GetAsync(
            b => b.Slug == slug && b.Status == EBlogStatus.Published,
            q => q.Include(b => b.BlogCategories),
            ct)
            ?? throw new KeyNotFoundException($"Bloq tapılmadı.");

        return MapToBlogDto(entity);
    }

    //  ===============================
    //  Admin operations
    //  ===============================

    public async Task<PagedResult<BlogDto>> GetAllAsync(BlogListQueryDto qDto, CancellationToken ct = default)
    {
        IQueryable<Blog> query = repository.GetAll(
            q => q.Include(b => b.BlogCategories));

        if (!string.IsNullOrWhiteSpace(qDto.Search))
        {
            var search = qDto.Search.Trim();

            query = query.Where(u =>
                (u.Title != null && u.Title.Contains(search))
            );
        }

        // Filter by status
        if (qDto.Status.HasValue)
        {
            query = query.Where(b => b.Status == qDto.Status.Value);
        }

        // Filter by category
        if (qDto.CategoryId.HasValue)
        {
            query = query.Where(b => b.BlogCategories.Any(bc => bc.Id == qDto.CategoryId.Value));
        }

        query = query.OrderByDescending(c => c.CreatedOn);

        var pageNumber = qDto.PageNumber <= 0 ? 1 : qDto.PageNumber;
        var pageSize = qDto.PageSize <= 0 ? 20 : qDto.PageSize;

        var pagedBlogs = await query.ToPagedResultAsync(pageNumber, pageSize, ct);

        var dtoItems = pagedBlogs.Items.Select(MapToBlogDto).ToList();

        return new PagedResult<BlogDto>
        {
            Items = dtoItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = pagedBlogs.TotalCount
        };
    }

    public async Task<bool> CreateAsync(CreateBlogDto createDto, CancellationToken ct = default)
    {
        if (createDto is null)
            throw new ArgumentNullException(nameof(createDto));

        var title = createDto.Title?.Trim();
        var content = createDto.Content?.Trim();

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Başlıq boş buraxıla bilməz.", nameof(createDto.Title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Məzmun boş buraxıla bilməz.", nameof(createDto.Content));

        var slug = SlugGenerator.GenerateSlug(title);

        var existingSlug = await repository.GetAsync(b => b.Slug == slug, null, ct);
        if (existingSlug is not null)
            throw new InvalidOperationException($"Bu başlıqla bloq artıq mövcuddur: '{title}'.");

        string? coverImageUrl = null;
        string? ogImageUrl = null;

        if (createDto.CoverImage is not null)
        {
            coverImageUrl = await fileService.UploadAsync(createDto.CoverImage, "blogs", ct);
        }

        if (createDto.OgImage is not null)
        {
            ogImageUrl = await fileService.UploadAsync(createDto.OgImage, "blogs", ct);
        }

        var entity = new Blog
        {
            Title = title,
            Slug = slug,
            Excerpt = createDto.Excerpt?.Trim(),
            Content = content,
            CoverImageUrl = coverImageUrl ?? string.Empty,
            OgImageUrl = ogImageUrl,
            Tags = BlogTagHelper.ToJson(createDto.Tags),
            MetaTitle = createDto.MetaTitle?.Trim(),
            MetaDescription = createDto.MetaDescription?.Trim(),
            CanonicalUrl = createDto.CanonicalUrl?.Trim(),
            Status = createDto.Status,
            PublishedAt = createDto.Status == EBlogStatus.Published ? DateTimeOffset.UtcNow : null
        };

        if (createDto.CategoryIds?.Any() == true)
        {
            var categories = await categoryRepository.GetAsync(
                c => createDto.CategoryIds.Contains(c.Id),
                null,
                ct);
            
            if (categories is not null)
            {
                entity.BlogCategories.Add(categories);
            }
        }

        await repository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> ModifyStatusAsync(Guid id, EBlogStatus status, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(typeof(EBlogStatus), status))
            throw new ArgumentException($"Yanlış status dəyəri: {status}.", nameof(status));

        var entity = await repository.GetByIdAsync(id, null, ct)
            ?? throw new KeyNotFoundException($"Bloq tapılmadı: '{id}'.");

        if (entity.Status == status)
            throw new InvalidOperationException($"Bu status artıq bloga təyin edilib.");

        entity.Status = status;

        if (status == EBlogStatus.Published && !entity.PublishedAt.HasValue)
        {
            entity.PublishedAt = DateTimeOffset.UtcNow;
        }

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateBlogDto updateDto, CancellationToken ct = default)
    {
        if (updateDto is null)
            throw new ArgumentNullException(nameof(updateDto));

        var title = updateDto.Title?.Trim();
        var content = updateDto.Content?.Trim();

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Başlıq boş buraxıla bilməz.", nameof(updateDto.Title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Məzmun boş buraxıla bilməz.", nameof(updateDto.Content));

        var entity = await repository.GetByIdAsync(id, q => q.Include(b => b.BlogCategories), ct)
            ?? throw new KeyNotFoundException($"Bloq tapılmadı: '{id}'.");

        var slug = SlugGenerator.GenerateSlug(title);

        if (entity.Slug != slug)
        {
            var existingSlug = await repository.GetAsync(b => b.Slug == slug && b.Id != id, null, ct);
            if (existingSlug is not null)
                throw new InvalidOperationException($"Bu başlıqla bloq artıq mövcuddur: '{title}'.");
        }

        // Handle cover image
        if (updateDto.DeleteCoverImage)
        {
            // Delete cover image
            if (!string.IsNullOrWhiteSpace(entity.CoverImageUrl))
            {
                await fileService.DeleteAsync(entity.CoverImageUrl, ct);
                entity.CoverImageUrl = string.Empty;
            }
        }
        else if (updateDto.CoverImage is not null)
        {
            // Replace cover image
            if (!string.IsNullOrWhiteSpace(entity.CoverImageUrl))
            {
                await fileService.DeleteAsync(entity.CoverImageUrl, ct);
            }
            entity.CoverImageUrl = await fileService.UploadAsync(updateDto.CoverImage, "blogs", ct);
        }

        // Handle OG image
        if (updateDto.DeleteOgImage)
        {
            // Delete OG image
            if (!string.IsNullOrWhiteSpace(entity.OgImageUrl))
            {
                await fileService.DeleteAsync(entity.OgImageUrl, ct);
                entity.OgImageUrl = null;
            }
        }
        else if (updateDto.OgImage is not null)
        {
            // Replace OG image
            if (!string.IsNullOrWhiteSpace(entity.OgImageUrl))
            {
                await fileService.DeleteAsync(entity.OgImageUrl, ct);
            }
            entity.OgImageUrl = await fileService.UploadAsync(updateDto.OgImage, "blogs", ct) ?? string.Empty;
        }

        entity.Title = title;
        entity.Slug = slug;
        entity.Excerpt = updateDto.Excerpt?.Trim();
        entity.Content = content;
        entity.Tags = BlogTagHelper.ToJson(updateDto.Tags);
        entity.MetaTitle = updateDto.MetaTitle?.Trim();
        entity.MetaDescription = updateDto.MetaDescription?.Trim();
        entity.CanonicalUrl = updateDto.CanonicalUrl?.Trim();
        entity.Status = updateDto.Status;

        if (updateDto.Status == EBlogStatus.Published && !entity.PublishedAt.HasValue)
        {
            entity.PublishedAt = DateTimeOffset.UtcNow;
        }

        entity.BlogCategories.Clear();

        if (updateDto.CategoryIds?.Any() == true)
        {
            var categories = await categoryRepository.GetAsync(
                c => updateDto.CategoryIds.Contains(c.Id),
                null,
                ct);
            
            if (categories is not null)
            {
                entity.BlogCategories.Add(categories);
            }
        }

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, null, ct)
            ?? throw new KeyNotFoundException($"Bloq tapılmadı: '{id}'.");

        if (!string.IsNullOrWhiteSpace(entity.CoverImageUrl))
        {
            await fileService.DeleteAsync(entity.CoverImageUrl, ct);
        }

        if (!string.IsNullOrWhiteSpace(entity.OgImageUrl))
        {
            await fileService.DeleteAsync(entity.OgImageUrl, ct);
        }

        repository.Remove(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    //  ===============================
    //  Private helpers
    //  ===============================

    private static BlogAllDto MapToBlogAllDto(Blog blog)
    {
        return new BlogAllDto
        {
            Id = blog.Id,
            Slug = blog.Slug,
            Title = blog.Title,
            Content = blog.Content,
            CoverImageUrl = blog.CoverImageUrl,
            Excerpt = blog.Excerpt,
            Tags = BlogTagHelper.FromJson(blog.Tags)?.ToList(),
            PublishedAt = blog.PublishedAt,
            Categories = blog.BlogCategories?.Select(bc => new BlogCategoryDto
            {
                Id = bc.Id,
                Name = bc.Name,
                Slug = bc.Slug
            }).ToList()
        };
    }

    private static BlogDto MapToBlogDto(Blog blog)
    {
        return new BlogDto
        {
            Id = blog.Id,
            Slug = blog.Slug,
            Title = blog.Title,
            Content = blog.Content,
            CoverImageUrl = blog.CoverImageUrl ?? string.Empty,
            OgImageUrl = blog.OgImageUrl,
            Excerpt = blog.Excerpt,
            Tags = BlogTagHelper.FromJson(blog.Tags),
            PublishedAt = blog.PublishedAt,
            MetaTitle = blog.MetaTitle,
            MetaDescription = blog.MetaDescription,
            CanonicalUrl = blog.CanonicalUrl,
            Status = blog.Status,
            Categories = blog.BlogCategories?.Select(bc => new BlogCategoryDto
            {
                Id = bc.Id,
                Name = bc.Name,
                Slug = bc.Slug
            }).ToList()
        };
    }
}
