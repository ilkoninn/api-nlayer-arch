using App.Core.Entities.Projects;
using ProjectE = App.Core.Entities.Projects.Project;

namespace App.Business.Services.Internal;

public class ProjectService(
    IGenericRepository<ProjectE, Guid> repository,
    IGenericRepository<ProjectCategory, Guid> categoryRepository,
    IGenericRepository<ProjectImage, Guid> imageRepository,
    IGenericRepository<ProjectFeature, Guid> featureRepository,
    IUnitOfWork unitOfWork,
    IFileService fileService) : IProjectService
{
    //  ===============================
    //  Public operations
    //  ===============================

    public async Task<PagedResult<ProjectAllDto>> GetAllPublicAsync(ProjectListQueryDto qDto, CancellationToken ct = default)
    {
        IQueryable<ProjectE> query = repository.GetAll(q => q
            .Include(p => p.ProjectCategory)
            .Include(p => p.Images));

        if (!string.IsNullOrWhiteSpace(qDto.Search))
        {
            var search = qDto.Search.Trim();

            query = query.Where(p =>
                (p.Title != null && p.Title.Contains(search)) ||
                (p.Excerpt != null && p.Excerpt.Contains(search))
            );
        }

        if (qDto.ProjectCategoryId.HasValue)
        {
            query = query.Where(p => p.ProjectCategoryId == qDto.ProjectCategoryId.Value);
        }

        query = query.OrderByDescending(p => p.CreatedOn);

        if (qDto.PageNumber <= 0 && qDto.PageSize <= 0)
        {
            var allProjects = await query.ToListAsync(ct);

            var allDtoItems = allProjects.Select(MapToProjectAllDto).ToList();

            return new PagedResult<ProjectAllDto>
            {
                Items = allDtoItems,
                PageNumber = 1,
                PageSize = allDtoItems.Count,
                TotalCount = allDtoItems.Count
            };
        }

        var pageNumber = qDto.PageNumber <= 0 ? 1 : qDto.PageNumber;
        var pageSize = qDto.PageSize <= 0 ? 20 : qDto.PageSize;

        var pagedProjects = await query.ToPagedResultAsync(pageNumber, pageSize, ct);

        var dtoItems = pagedProjects.Items.Select(MapToProjectAllDto).ToList();

        return new PagedResult<ProjectAllDto>
        {
            Items = dtoItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = pagedProjects.TotalCount
        };
    }

    public async Task<ProjectDto> GetDetailBySlugAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug boş ola bilməz.", nameof(slug));

        var entity = await repository.GetAsync(
            p => p.Slug == slug,
            q => q.Include(p => p.ProjectCategory)
                  .Include(p => p.Images)
                  .Include(p => p.Features),
            ct)
            ?? throw new KeyNotFoundException($"Layihə tapılmadı.");

        return MapToProjectDto(entity);
    }

    //  ===============================
    //  Admin operations
    //  ===============================

    public async Task<PagedResult<ProjectDto>> GetAllAsync(ProjectListQueryDto qDto, CancellationToken ct = default)
    {
        IQueryable<ProjectE> query = repository.GetAll(q => q
            .Include(p => p.ProjectCategory)
            .Include(p => p.Images)
            .Include(p => p.Features));

        if (!string.IsNullOrWhiteSpace(qDto.Search))
        {
            var search = qDto.Search.Trim();

            query = query.Where(p =>
                (p.Title != null && p.Title.Contains(search)) ||
                (p.Excerpt != null && p.Excerpt.Contains(search))
            );
        }

        if (qDto.ProjectCategoryId.HasValue)
        {
            query = query.Where(p => p.ProjectCategoryId == qDto.ProjectCategoryId.Value);
        }

        query = query.OrderByDescending(p => p.CreatedOn);

        var pageNumber = qDto.PageNumber <= 0 ? 1 : qDto.PageNumber;
        var pageSize = qDto.PageSize <= 0 ? 20 : qDto.PageSize;

        var pagedProjects = await query.ToPagedResultAsync(pageNumber, pageSize, ct);

        var dtoItems = pagedProjects.Items.Select(MapToProjectDto).ToList();

        return new PagedResult<ProjectDto>
        {
            Items = dtoItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = pagedProjects.TotalCount
        };
    }

    public async Task<bool> CreateAsync(CreateProjectDto createDto, CancellationToken ct = default)
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

        var category = await categoryRepository.GetByIdAsync(createDto.ProjectCategoryId, null, ct)
            ?? throw new KeyNotFoundException($"Kateqoriya tapılmadı: '{createDto.ProjectCategoryId}'.");

        var slug = SlugGenerator.GenerateSlug(title);

        var existingSlug = await repository.GetAsync(p => p.Slug == slug, null, ct);
        if (existingSlug is not null)
            throw new InvalidOperationException($"Bu başlıqla layihə artıq mövcuddur: '{title}'.");

        var entity = new ProjectE
        {
            Title = title,
            Slug = slug,
            Excerpt = excerpt,
            Content = content,
            ProjectCategoryId = createDto.ProjectCategoryId,
            MetaTitle = createDto.MetaTitle?.Trim(),
            MetaDescription = createDto.MetaDescription?.Trim(),
            CanonicalUrl = createDto.CanonicalUrl?.Trim()
        };

        if (createDto.Images?.Any() == true)
        {
            foreach (var imageFile in createDto.Images)
            {
                var imageUrl = await fileService.UploadAsync(imageFile, "projects", ct);
                entity.Images.Add(new ProjectImage { ImageUrl = imageUrl });
            }
        }

        if (createDto.Features?.Any() == true)
        {
            foreach (var feature in createDto.Features)
            {
                var key = feature.Key?.Trim();
                var value = feature.Value?.Trim();

                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                {
                    entity.Features.Add(new ProjectFeature { Key = key, Value = value });
                }
            }
        }

        await repository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateProjectDto updateDto, CancellationToken ct = default)
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

        var entity = await repository.GetAsync(
            p => p.Id == id,
            q => q.Include(p => p.Images).Include(p => p.Features),
            ct)
            ?? throw new KeyNotFoundException($"Layihə tapılmadı: '{id}'.");

        var category = await categoryRepository.GetByIdAsync(updateDto.ProjectCategoryId, null, ct)
            ?? throw new KeyNotFoundException($"Kateqoriya tapılmadı: '{updateDto.ProjectCategoryId}'.");

        var slug = SlugGenerator.GenerateSlug(title);

        if (entity.Slug != slug)
        {
            var existingSlug = await repository.GetAsync(p => p.Slug == slug && p.Id != id, null, ct);
            if (existingSlug is not null)
                throw new InvalidOperationException($"Bu başlıqla layihə artıq mövcuddur: '{title}'.");
        }

        entity.Title = title;
        entity.Slug = slug;
        entity.Excerpt = excerpt;
        entity.Content = content;
        entity.ProjectCategoryId = updateDto.ProjectCategoryId;
        entity.MetaTitle = updateDto.MetaTitle?.Trim();
        entity.MetaDescription = updateDto.MetaDescription?.Trim();
        entity.CanonicalUrl = updateDto.CanonicalUrl?.Trim();

        // Remove specific images by IDs if provided
        if (updateDto.RemoveImageIds?.Any() == true)
        {
            var imagesToRemove = entity.Images.Where(img => updateDto.RemoveImageIds.Contains(img.Id)).ToList();
            
            foreach (var imageToRemove in imagesToRemove)
            {
                await fileService.DeleteAsync(imageToRemove.ImageUrl, ct);
                imageRepository.Remove(imageToRemove);
                entity.Images.Remove(imageToRemove);
            }
        }

        // Add new images if provided
        if (updateDto.Images?.Any() == true)
        {
            foreach (var imageFile in updateDto.Images)
            {
                var imageUrl = await fileService.UploadAsync(imageFile, "projects", ct);
                entity.Images.Add(new ProjectImage { ImageUrl = imageUrl });
            }
        }

        // Always clear and rebuild features
        var oldFeatures = entity.Features.ToList();
        foreach (var oldFeature in oldFeatures)
        {
            featureRepository.Remove(oldFeature);
        }

        entity.Features.Clear();

        if (updateDto.Features?.Any() == true)
        {
            foreach (var feature in updateDto.Features)
            {
                var key = feature.Key?.Trim();
                var value = feature.Value?.Trim();

                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                {
                    entity.Features.Add(new ProjectFeature { Key = key, Value = value });
                }
            }
        }

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await repository.GetAsync(
            p => p.Id == id,
            q => q.Include(p => p.Images).Include(p => p.Features),
            ct)
            ?? throw new KeyNotFoundException($"Layihə tapılmadı: '{id}'.");

        foreach (var image in entity.Images)
        {
            await fileService.DeleteAsync(image.ImageUrl, ct);
        }

        repository.Remove(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    //  ===============================
    //  Private mapping
    //  ===============================

    private static ProjectAllDto MapToProjectAllDto(ProjectE entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Slug = entity.Slug,
        Excerpt = entity.Excerpt,
        Category = new ProjectCategoryDto
        {
            Id = entity.ProjectCategory.Id,
            Name = entity.ProjectCategory.Name,
            Slug = entity.ProjectCategory.Slug,
        },
        CoverImageUrl = entity.Images.FirstOrDefault()?.ImageUrl
    };

    private static ProjectDto MapToProjectDto(ProjectE entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Slug = entity.Slug,
        Excerpt = entity.Excerpt,
        Content = entity.Content,
        MetaTitle = entity.MetaTitle,
        MetaDescription = entity.MetaDescription,
        CanonicalUrl = entity.CanonicalUrl,
        Category = new ProjectCategoryDto
        {
            Id = entity.ProjectCategory.Id,
            Name = entity.ProjectCategory.Name,
            Slug = entity.ProjectCategory.Slug,
        },
        Images = entity.Images.Select(i => new ProjectImageDto
        {
            Id = i.Id,
            ImageUrl = i.ImageUrl
        }).ToList(),
        Features = entity.Features.Select(f => new ProjectFeatureDto
        {
            Id= f.Id,
            Key = f.Key,
            Value = f.Value
        }).ToList()
    };
}