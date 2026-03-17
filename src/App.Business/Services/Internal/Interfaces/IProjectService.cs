namespace App.Business.Services.Internal.Interfaces;

public interface IProjectService
{ 
    //  ===============================
    //  Public operations
    //  ===============================

    Task<PagedResult<ProjectAllDto>> GetAllPublicAsync(ProjectListQueryDto qDto, CancellationToken ct = default);
    Task<ProjectDto> GetDetailBySlugAsync(string slug, CancellationToken ct = default);


    //  ===============================
    //  Admin operations
    //  ===============================
    Task<PagedResult<ProjectDto>> GetAllAsync(ProjectListQueryDto qDto, CancellationToken ct = default);
    Task<bool> CreateAsync(CreateProjectDto createDto, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, UpdateProjectDto updateDto, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}