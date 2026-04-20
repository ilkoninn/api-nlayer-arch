namespace App.Core.Entities.Common;

public interface IRepositoryEntity<TId>
{
	TId Id { get; }
	bool IsDeleted { get; set; }
	bool IsActive { get; set; }
}
