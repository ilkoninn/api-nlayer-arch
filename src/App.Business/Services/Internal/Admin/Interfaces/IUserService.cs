namespace App.Business.Services.Internal.Admin.Interfaces;

public interface IUserService
{
    //  ===============================
    //  Admin operations
    //  ===============================

    Task<PagedResult<UserResponseDto>> GetAllAsync(UserListQueryDto query, CancellationToken ct = default);
	Task<UserResponseDto> GetByIdAsync(Guid Id, CancellationToken ct = default);
	Task CreateAsync(CreateUserDto createUserDTO, CancellationToken ct = default);
	Task UpdateAsync(Guid id, UpdateUserDto updateUserDTO, CancellationToken ct = default);
	Task DeleteAsync(Guid id, CancellationToken ct = default);
	Task BanAsync(Guid id, CancellationToken ct = default);
	Task UnBanAsync(Guid id, CancellationToken ct = default);
	Task AssignRoleAsync(Guid userId, string roleName, CancellationToken ct = default);
	Task RemoveRoleAsync(Guid userId, string roleName, CancellationToken ct = default);
}


