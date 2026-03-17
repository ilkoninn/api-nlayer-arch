namespace App.DAL.Repositories.Interfaces;

public interface IUnitOfWork
{
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
