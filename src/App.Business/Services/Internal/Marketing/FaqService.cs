namespace App.Business.Services.Internal;

public class FaqService(
    IGenericRepository<Faq, Guid> repository,
    IUnitOfWork unitOfWork) : IFaqService
{

    //  ===============================
    //  Public operations
    //  ===============================

    public async Task<PagedResult<FaqDto>> GetAllAsync(FaqListQueryDto faqListQuery, CancellationToken ct = default)
    {
        IQueryable<Faq> query = repository.GetAll();

        if (!string.IsNullOrWhiteSpace(faqListQuery.Search))
        {
            var search = faqListQuery.Search.Trim();

            query = query.Where(u =>
                (u.Question != null && u.Question.Contains(search)) ||
                (u.Answer != null && u.Answer.Contains(search))
            );
        }

        query = query.OrderByDescending(c => c.CreatedOn);

        if (faqListQuery.PageNumber <= 0 && faqListQuery.PageSize <= 0)
        {
            var allFaqs = await query.ToListAsync(ct);
            
            var allDtoItems = allFaqs.Select(faq => new FaqDto
            {
                Id = faq.Id,
                Question = faq.Question,
                Answer = faq.Answer,
            }).ToList();

            return new PagedResult<FaqDto>
            {
                Items = allDtoItems,
                PageNumber = 1,
                PageSize = allDtoItems.Count,
                TotalCount = allDtoItems.Count
            };
        }

        var pageNumber = faqListQuery.PageNumber <= 0 ? 1 : faqListQuery.PageNumber;
        var pageSize = faqListQuery.PageSize <= 0 ? 20 : faqListQuery.PageSize;

        var pagedFaqs = await query.ToPagedResultAsync(pageNumber, pageSize, ct);

        var dtoItems = pagedFaqs.Items.Select(faq => new FaqDto
        {
            Id = faq.Id,
            Question = faq.Question,
            Answer = faq.Answer,
        }).ToList();

        return new PagedResult<FaqDto>
        {
            Items = dtoItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = pagedFaqs.TotalCount
        };
    }

    //  ===============================
    //  Admin operations
    //  ===============================

    public async Task<bool> CreateAsync(CreateFaqDto createDto, CancellationToken ct = default)
    {
        if (createDto is null)
            throw new ArgumentNullException(nameof(createDto));

        var question = createDto.Question?.Trim();
        var answer = createDto.Answer?.Trim();

        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Sual boş buraxıla bilməz.", nameof(createDto.Question));

        if (string.IsNullOrWhiteSpace(answer))
            throw new ArgumentException("Cavab boş buraxıla bilməz.", nameof(createDto.Answer));

        var faq = new Faq
        {
            Question = question!,
            Answer = answer!,
        };

        await repository.AddAsync(faq, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateFaqDto updateDto, CancellationToken ct = default)
    {
        if (updateDto is null)
            throw new ArgumentNullException(nameof(updateDto));

        var question = updateDto.Question?.Trim();
        var answer = updateDto.Answer?.Trim();

        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Sual boş buraxıla bilməz.", nameof(updateDto.Question));

        if (string.IsNullOrWhiteSpace(answer))
            throw new ArgumentException("Cavab boş buraxıla bilməz.", nameof(updateDto.Answer));

        var entity = await repository.GetByIdAsync(id, null, ct)
            ?? throw new KeyNotFoundException($"Sual tapılmadı: '{id}'.");

        entity.Question = question!;
        entity.Answer = answer!;

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, null, ct)
            ?? throw new KeyNotFoundException($"Sual tapılmadı: '{id}'.");

        repository.Remove(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
