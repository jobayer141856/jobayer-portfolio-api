using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Contracts;

namespace Portfolio.Infrastructure.Data;

public class Repository<TAggregateRoot, TKey> : IRepository<TAggregateRoot, TKey>
    where TAggregateRoot : class, IAggregateRoot<TKey>
    where TKey : IComparable
{
    protected readonly DbContext _context;
    protected readonly DbSet<TAggregateRoot> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TAggregateRoot>();
    }

    public virtual void Add(TAggregateRoot entity) => _dbSet.Add(entity);
    public virtual async Task AddAsync(TAggregateRoot entity, CancellationToken cancellationToken = default) =>
        await _dbSet.AddAsync(entity, cancellationToken);

    public virtual void Edit(TAggregateRoot entityToUpdate) => _dbSet.Update(entityToUpdate);
    public virtual Task EditAsync(TAggregateRoot entityToUpdate, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entityToUpdate);
        return Task.CompletedTask;
    }

    public virtual IList<TAggregateRoot> GetAll() => _dbSet.ToList();
    public virtual async Task<IList<TAggregateRoot>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbSet.ToListAsync(cancellationToken);

    public virtual TAggregateRoot? GetById(TKey id) => _dbSet.Find(id);
    public virtual async Task<TAggregateRoot?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default) =>
        await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    public virtual int GetCount(Expression<Func<TAggregateRoot, bool>>? filter = null) =>
        filter == null ? _dbSet.Count() : _dbSet.Count(filter);

    public virtual async Task<int> GetCountAsync(Expression<Func<TAggregateRoot, bool>>? filter = null, CancellationToken cancellationToken = default) =>
        filter == null ? await _dbSet.CountAsync(cancellationToken) : await _dbSet.CountAsync(filter, cancellationToken);

    public virtual void Remove(Expression<Func<TAggregateRoot, bool>> filter) =>
        _dbSet.RemoveRange(_dbSet.Where(filter));

    public virtual void Remove(TAggregateRoot entityToDelete) => _dbSet.Remove(entityToDelete);

    public virtual void Remove(TKey id)
    {
        var entity = GetById(id);
        if (entity != null) Remove(entity);
    }

    public virtual Task RemoveAsync(Expression<Func<TAggregateRoot, bool>>? filter, CancellationToken cancellationToken = default)
    {
        if (filter != null)
            _dbSet.RemoveRange(_dbSet.Where(filter));
        return Task.CompletedTask;
    }

    public virtual Task RemoveAsync(TAggregateRoot entityToDelete, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entityToDelete);
        return Task.CompletedTask;
    }

    public virtual async Task RemoveAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null) _dbSet.Remove(entity);
    }

    public virtual IQueryable<TAggregateRoot> Query() => _dbSet;
}

