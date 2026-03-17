namespace App.Business.Services.Internal.Interfaces;

public interface IBlogCategoryService
{
    //  ===============================
    //  Public operations
    //  ===============================

    Task<PagedResult<BlogCategoryDto>> GetAllAsync(BlogCategoryListQueryDto query, CancellationToken ct = default);


    //  ===============================
    //  Admin operations
    //  ===============================

    Task<bool> CreateAsync(CreateBlogCategoryDto createDto, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, UpdateBlogCategoryDto updateDto, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
