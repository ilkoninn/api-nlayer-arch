namespace App.DAL.Repositories.Interfaces;

public interface IGenericRepository<TEntity, TId>
	where TEntity : AuditableEntity
{
	// =====================================================================
	// Read
	// =====================================================================

	Task<TEntity?> GetByIdAsync(
		TId id,
		Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
		CancellationToken ct = default);

	Task<TEntity?> GetAsync(
		Expression<Func<TEntity, bool>> predicate,
		Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
		CancellationToken ct = default);

	IQueryable<TEntity> GetAll(
		Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);

	IQueryable<TEntity> GetWhere(
		Expression<Func<TEntity, bool>> predicate,
		Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
		CancellationToken ct = default);

	Task<bool> AnyAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken ct = default);

	Task<int> CountAsync(
		Expression<Func<TEntity, bool>>? predicate = null,
		CancellationToken ct = default);

	// =====================================================================
	// Write
	// =====================================================================

	Task AddAsync(TEntity entity, CancellationToken ct = default);
	Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

	void Update(TEntity entity);
	void UpdateRange(IEnumerable<TEntity> entities);

	void Activate(TEntity entity);
	void Deactivate(TEntity entity);

	void ActivateRange(IEnumerable<TEntity> entities);
	void DeactivateRange(IEnumerable<TEntity> entities);

	void SoftDelete(TEntity entity);
	void SoftDeleteRange(IEnumerable<TEntity> entities);

	void Restore(TEntity entity);
	void RestoreRange(IEnumerable<TEntity> entities);

	void Remove(TEntity entity);
	void RemoveRange(IEnumerable<TEntity> entities);
}