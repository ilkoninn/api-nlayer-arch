namespace App.Business.Services.Internal.Interfaces;

public interface ISettingService
{
    //  ===============================
    //  Public operations
    //  ===============================

    Task<PagedResult<SettingDto>> GetAllPublicAsync(SettingListQueryDto query, CancellationToken ct = default);


    //  ===============================
    //  Admin operations
    //  ===============================

    Task<bool> CreateAsync(CreateSettingDto createDto, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, UpdateSettingDto updateDto, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
