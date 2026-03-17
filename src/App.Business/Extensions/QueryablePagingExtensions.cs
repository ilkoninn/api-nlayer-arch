using App.Core.Entities.Common;

namespace App.Business.Extensions;

public static class QueryablePagingExtensions
{
	public static async Task<PagedResult<TEntity>> ToPagedResultAsync<TEntity>(
	   this IQueryable<TEntity> query,
	   int pageNumber = 1,
	   int pageSize = 20,
	   CancellationToken ct = default)
	{
		if (pageNumber < 1) pageNumber = 1;
		if (pageSize < 1) pageSize = 20;
		if (pageSize > 100) pageSize = 100; 

		var totalCount = await query.CountAsync(ct);

		var items = await query
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(ct);

		return new PagedResult<TEntity>
		{
			Items = items,
			PageNumber = pageNumber,
			PageSize = pageSize,
			TotalCount = totalCount
		};
	}
}
