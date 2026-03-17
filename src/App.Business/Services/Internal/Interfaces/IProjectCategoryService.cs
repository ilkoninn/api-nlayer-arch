namespace App.Business.Services.Internal.Interfaces;

public interface IProjectCategoryService
{
    Task<PagedResult<ProjectCategoryDto>> GetAllAsync(ProjectCategoryListQueryDto qDto, CancellationToken ct = default);
    Task<bool> CreateAsync(CreateProjectCategoryDto createDto, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, UpdateProjectCategoryDto updateDto, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}