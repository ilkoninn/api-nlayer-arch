using App.Core.Entities.Projects;

namespace App.Business.Services.Internal;

public class ProjectCategoryService(
    IGenericRepository<ProjectCategory, Guid> repository,
    IUnitOfWork unitOfWork) : IProjectCategoryService
{
    //  ===============================
    //  Public operations
    //  ===============================

    public async Task<PagedResult<ProjectCategoryDto>> GetAllAsync(ProjectCategoryListQueryDto qDto, CancellationToken ct = default)
    {
        IQueryable<ProjectCategory> query = repository.GetAll();

        if (!string.IsNullOrWhiteSpace(qDto.Search))
        {
            var search = qDto.Search.Trim();

            query = query.Where(c =>
                (c.Name != null && c.Name.Contains(search))
            );
        }

        query = query.OrderByDescending(c => c.CreatedOn);

        if (qDto.PageNumber <= 0 && qDto.PageSize <= 0)
        {
            var allCategories = await query.ToListAsync(ct);

            var allDtoItems = allCategories.Select(category => new ProjectCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
            }).ToList();

            return new PagedResult<ProjectCategoryDto>
            {
                Items = allDtoItems,
                PageNumber = 1,
                PageSize = allDtoItems.Count,
                TotalCount = allDtoItems.Count
            };
        }

        var pageNumber = qDto.PageNumber <= 0 ? 1 : qDto.PageNumber;
        var pageSize = qDto.PageSize <= 0 ? 20 : qDto.PageSize;

        var pagedCategories = await query.ToPagedResultAsync(pageNumber, pageSize, ct);

        var dtoItems = pagedCategories.Items.Select(category => new ProjectCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
        }).ToList();

        return new PagedResult<ProjectCategoryDto>
        {
            Items = dtoItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = pagedCategories.TotalCount
        };
    }

    //  ===============================
    //  Admin operations
    //  ===============================

    public async Task<bool> CreateAsync(CreateProjectCategoryDto createDto, CancellationToken ct = default)
    {
        if (createDto is null)
            throw new ArgumentNullException(nameof(createDto));

        var name = createDto.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Kateqoriya adı boş buraxıla bilməz.", nameof(createDto.Name));

        var slug = SlugGenerator.GenerateSlug(name);

        var existingBySlug = await repository.GetAsync(c => c.Slug == slug, null, ct);
        if (existingBySlug is not null)
            throw new InvalidOperationException($"Bu adla kateqoriya artıq mövcuddur: '{name}'.");

        var newEntity = new ProjectCategory
        {
            Name = name,
            Slug = slug,
        };

        await repository.AddAsync(newEntity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateProjectCategoryDto updateDto, CancellationToken ct = default)
    {
        if (updateDto is null)
            throw new ArgumentNullException(nameof(updateDto));

        var name = updateDto.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Kateqoriya adı boş buraxıla bilməz.", nameof(updateDto.Name));

        var entity = await repository.GetByIdAsync(id, null, ct)
            ?? throw new KeyNotFoundException($"Layihə kateqoriyası tapılmadı: '{id}'.");

        var slug = SlugGenerator.GenerateSlug(name);

        if (entity.Slug != slug)
        {
            var existingBySlug = await repository.GetAsync(c => c.Slug == slug && c.Id != id, null, ct);
            if (existingBySlug is not null)
                throw new InvalidOperationException($"Bu adla kateqoriya artıq mövcuddur: '{name}'.");
        }

        entity.Name = name;
        entity.Slug = slug;

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, null, ct)
            ?? throw new KeyNotFoundException($"Layihə kateqoriyası tapılmadı: '{id}'.");

        repository.Remove(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}