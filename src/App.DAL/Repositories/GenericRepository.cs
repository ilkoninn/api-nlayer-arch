namespace App.DAL.Repositories;

public class GenericRepository<TEntity, TId>(AppDbContext context)
	: IGenericRepository<TEntity, TId>
	where TEntity : AuditableEntity
{
	protected readonly AppDbContext Context = context;
	protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

	// =====================================================================
	// Read
	// =====================================================================

	public async Task<TEntity?> GetByIdAsync(
		TId id,
		Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
		CancellationToken ct = default)
	{
		var query = DbSet.AsQueryable();

		if (include != null)
			query = include(query);

		return await query.FirstOrDefaultAsync(
			e => e.Id.Equals(id),
			ct);
	}

	public async Task<TEntity?> GetAsync(
		Expression<Func<TEntity, bool>> predicate,
		Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
		CancellationToken ct = default)
	{
		var query = DbSet.AsQueryable();

		if (include != null)
			query = include(query);

		return await query.FirstOrDefaultAsync(
			CombineWithNotDeleted(predicate),
			ct);
	}

	public IQueryable<TEntity> GetAll(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
	{
		var query = DbSet.AsQueryable().AsNoTracking();

		if (include != null)
			query = include(query);

		return query;
	}

	public IQueryable<TEntity> GetWhere(
		Expression<Func<TEntity, bool>> predicate,
		Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
		CancellationToken ct = default)
	{
		var query = DbSet.AsQueryable().AsNoTracking();

		if (include != null)
			query = include(query);

		query = query.Where(predicate);

		return query;
	}

	public async Task<bool> AnyAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken ct = default)
	{
		return await DbSet.AnyAsync(
			CombineWithNotDeleted(predicate),
			ct);
	}

	public async Task<int> CountAsync(
		Expression<Func<TEntity, bool>>? predicate = null,
		CancellationToken ct = default)
	{
		var query = DbSet.Where(e => !e.IsDeleted);

		return predicate == null
			? await query.CountAsync(ct)
			: await query.CountAsync(predicate, ct);
	}

	// =====================================================================
	// Write
	// =====================================================================

	public async Task AddAsync(TEntity entity, CancellationToken ct = default)
	{
		await DbSet.AddAsync(entity, ct);
	}

	public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
	{
		await DbSet.AddRangeAsync(entities, ct);
	}

	public void Update(TEntity entity)
	{
		DbSet.Update(entity);
	}

	public void UpdateRange(IEnumerable<TEntity> entities)
	{
		DbSet.UpdateRange(entities);
	}

	public void Activate(TEntity entity)
	{
		entity.IsActive = true;
		DbSet.Update(entity);
	}

	public void ActivateRange(IEnumerable<TEntity> entities)
	{
		foreach (var entity in entities)
		{
			entity.IsActive = true;
		}

		DbSet.UpdateRange(entities);
	}

	public void Deactivate(TEntity entity)
	{
		entity.IsActive = false;
		DbSet.Update(entity);
	}

	public void DeactivateRange(IEnumerable<TEntity> entities)
	{
		foreach (var entity in entities)
		{
			entity.IsActive = false;
		}

		DbSet.UpdateRange(entities);
	}

	public void SoftDelete(TEntity entity)
	{
		entity.IsDeleted = true;
		entity.IsActive = false;
		DbSet.Update(entity);
	}

	public void SoftDeleteRange(IEnumerable<TEntity> entities)
	{
		foreach (var entity in entities)
		{
			entity.IsDeleted = true;
			entity.IsActive = false;
		}

		DbSet.UpdateRange(entities);
	}

	public void Restore(TEntity entity)
	{
		entity.IsDeleted = false;
		DbSet.Update(entity);
	}

	public void RestoreRange(IEnumerable<TEntity> entities)
	{
		foreach (var entity in entities)
		{
			entity.IsDeleted = false;
		}

		DbSet.UpdateRange(entities);
	}

	public void Remove(TEntity entity)
	{
		DbSet.Remove(entity);
	}

	public void RemoveRange(IEnumerable<TEntity> entities)
	{
		DbSet.RemoveRange(entities);
	}

	// =====================================================================
	// Helpers
	// =====================================================================

	protected static Expression<Func<TEntity, bool>> CombineWithNotDeleted(
		Expression<Func<TEntity, bool>> predicate)
	{
		var parameter = predicate.Parameters[0];
		var notDeleted = Expression.Not(Expression.Property(parameter, "IsDeleted"));
		var body = Expression.AndAlso(notDeleted, predicate.Body);
		return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
	}
}
