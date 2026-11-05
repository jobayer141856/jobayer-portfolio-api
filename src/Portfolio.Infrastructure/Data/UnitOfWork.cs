using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Contracts;

namespace Portfolio.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    protected readonly DbContext _dbContext;

    public UnitOfWork(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Save() => _dbContext.SaveChanges();

    public async Task SaveAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}

