namespace App.Business.Services.Internal;

public sealed class ContactService(
    IGenericRepository<Contact, Guid> repository,
    IUnitOfWork unitOfWork) : IContactService
{
    //  ===============================
    //  Public operations
    //  ===============================

    public async Task<bool> CreateAsync(CreateContactDto createDto, CancellationToken ct = default)
    {
        if (createDto is null)
            throw new ArgumentNullException(nameof(createDto));

        var fullName = createDto.FullName?.Trim();
        var message = createDto.Message?.Trim();
        var email = createDto.Email?.Trim();
        var phone = createDto.PhoneNumber?.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Ad və soyad boş buraxıla bilməz.", nameof(createDto.FullName));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Mesaj boş buraxıla bilməz.", nameof(createDto.Message));

        if (!string.IsNullOrWhiteSpace(email))
        {
            try 
            { 
                _ = new MailAddress(email); 
            }
            catch (Exception ex) 
            { 
                throw new ArgumentException("Email formatı düzgün deyil.", nameof(createDto.Email), ex); 
            }
        }

        var entity = new Contact
        {
            FullName = fullName!,
            Message = message!,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone,
            Status = EContactStatus.New
        };

        await repository.AddAsync(entity, ct);
        return await unitOfWork.SaveChangesAsync(ct) > 0;
    }


    //  ===============================
    //  Admin operations
    //  ===============================

    public async Task<PagedResult<ContactDto>> GetAllAsync(ContactListQueryDto qDto, CancellationToken ct = default)
    {
        IQueryable<Contact> query = repository.GetAll();

        if (!string.IsNullOrWhiteSpace(qDto.Search))
        {
            var search = qDto.Search.Trim();

            query = query.Where(u =>
                (u.Message != null && u.Message.Contains(search)) ||
                (u.FullName != null && u.FullName.Contains(search)) ||
                (u.Email != null && u.Email.Contains(search))
            );
        }

        // Filter by status
        if (qDto.Status.HasValue)
        {
            query = query.Where(b => b.Status == qDto.Status.Value);
        }

        query = query.OrderByDescending(c => c.CreatedOn);

        var pageNumber = qDto.PageNumber <= 0 ? 1 : qDto.PageNumber;
        var pageSize = qDto.PageSize <= 0 ? 20 : qDto.PageSize;

        var pagedContacts = await query.ToPagedResultAsync(pageNumber, pageSize, ct);

        var dtoItems = pagedContacts.Items.Select(contact => new ContactDto
        {
            Id = contact.Id,
            FullName = contact.FullName ?? string.Empty,
            Message = contact.Message,
            PhoneNumber = contact.PhoneNumber,
            Email = contact.Email,
            ContactStatus = contact.Status,
            ViewedAt = contact.ViewedAt
        }).ToList();

        return new PagedResult<ContactDto>
        {
            Items = dtoItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = pagedContacts.TotalCount
        };
    }

    public async Task<ContactDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var contact = await repository.GetByIdAsync(id, null, ct);

        if (contact is null)
            throw new KeyNotFoundException($"Əlaqə tapılmadı: '{id}'.");

        return new ContactDto
        {
            Id = contact.Id,
            FullName = contact.FullName ?? string.Empty,
            Message = contact.Message,
            PhoneNumber = contact.PhoneNumber,
            Email = contact.Email,
            ContactStatus = contact.Status,
            ViewedAt = contact.ViewedAt,
        };
    }

    public async Task<bool> ModifyStatusAsync(Guid id, EContactStatus status, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(typeof(EContactStatus), status))
            throw new ArgumentException($"Yanlış status dəyəri: {status}.", nameof(status));

        var contact = await repository.GetByIdAsync(id, null, ct);

        if (contact is null)
            throw new KeyNotFoundException($"Əlaqə tapılmadı: '{id}'.");

        if (contact.Status == status)
            throw new InvalidOperationException($"Bu status artıq əlaqəyə təyin edilib.");

        contact.Status = status;
        contact.ViewedAt = DateTime.UtcNow;

        repository.Update(contact);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var contact = await repository.GetByIdAsync(id, null, ct);

        if (contact is null)
            throw new KeyNotFoundException($"Əlaqə tapılmadı: '{id}'.");

        repository.Remove(contact);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
