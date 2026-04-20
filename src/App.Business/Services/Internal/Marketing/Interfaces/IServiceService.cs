namespace App.Business.Services.Internal.Marketing.Interfaces;

public interface IServiceService
{ 
    //  ===============================
    //  Public operations
    //  ===============================

    Task<PagedResult<ServiceAllDto>> GetAllPublicAsync(ServiceListQueryDto qDto, CancellationToken ct = default);
    Task<ServiceDto> GetDetailBySlugAsync(string slug, CancellationToken ct = default);


    //  ===============================
    //  Admin operations
    //  ===============================
    Task<PagedResult<ServiceDto>> GetAllAsync(ServiceListQueryDto qDto, CancellationToken ct = default);
    Task<bool> CreateAsync(CreateServiceDto createDto, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, UpdateServiceDto updateDto, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}


