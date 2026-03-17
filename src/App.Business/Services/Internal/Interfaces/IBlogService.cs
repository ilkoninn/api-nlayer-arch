namespace App.Business.Services.Internal.Interfaces;

public interface IBlogService
{
    //  ===============================
    //  Public operations
    //  ===============================

    Task<PagedResult<BlogAllDto>> GetAllPublicAsync(BlogListQueryDto qDto, CancellationToken ct = default);
    Task<BlogDto> GetDetailBySlugAsync(string slug, CancellationToken ct = default);


    //  ===============================
    //  Admin operations
    //  ===============================
    Task<PagedResult<BlogDto>> GetAllAsync(BlogListQueryDto qDto, CancellationToken ct = default);
    Task<bool> CreateAsync(CreateBlogDto createDto, CancellationToken ct = default);
    Task<bool> ModifyStatusAsync(Guid id, EBlogStatus status, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, UpdateBlogDto updateDto, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
