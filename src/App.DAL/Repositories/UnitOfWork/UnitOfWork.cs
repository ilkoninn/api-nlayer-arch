namespace App.DAL.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public int SaveChanges() 
        => context.SaveChanges();
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => await context.Database.BeginTransactionAsync(ct);
}
