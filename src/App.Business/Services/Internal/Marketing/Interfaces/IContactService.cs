namespace App.Business.Services.Internal.Marketing.Interfaces;

public interface IContactService
{
	//  ===============================
	//  Public operations
	//  ===============================

	Task<bool> CreateAsync(CreateContactDto createDto, CancellationToken ct = default);

	//  ===============================
	//  Admin operations
	//  ===============================

	Task<PagedResult<ContactDto>> GetAllAsync(ContactListQueryDto contactListQuery, CancellationToken ct = default);
	Task<ContactDto> GetByIdAsync(Guid id, CancellationToken ct = default);
	Task<bool> ModifyStatusAsync(Guid id, EContactStatus status, CancellationToken ct = default);
	Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}


