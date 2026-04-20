namespace App.Business.Services.Internal.Marketing.Interfaces;

public interface IFaqService
{
    //  ===============================
    //  Public operations
    //  ===============================

    Task<PagedResult<FaqDto>> GetAllAsync(FaqListQueryDto query, CancellationToken ct = default);


    //  ===============================
    //  Admin operations
    //  ===============================

    Task<bool> CreateAsync(CreateFaqDto createDto, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, UpdateFaqDto updateDto, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}


