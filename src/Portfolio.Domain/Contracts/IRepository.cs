using System.Linq.Expressions;

namespace Portfolio.Domain.Contracts;

public interface IRepository<TAggregateRoot, TKey>
    where TAggregateRoot : class, IAggregateRoot<TKey>
    where TKey : IComparable
{
    void Add(TAggregateRoot entity);
    Task AddAsync(TAggregateRoot entity, CancellationToken cancellationToken = default);
    void Edit(TAggregateRoot entityToUpdate);
    Task EditAsync(TAggregateRoot entityToUpdate, CancellationToken cancellationToken = default);
    IList<TAggregateRoot> GetAll();
    Task<IList<TAggregateRoot>> GetAllAsync(CancellationToken cancellationToken = default);
    TAggregateRoot? GetById(TKey id);
    Task<TAggregateRoot?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    int GetCount(Expression<Func<TAggregateRoot, bool>>? filter = null);
    Task<int> GetCountAsync(Expression<Func<TAggregateRoot, bool>>? filter = null, CancellationToken cancellationToken = default);
    void Remove(Expression<Func<TAggregateRoot, bool>> filter);
    void Remove(TAggregateRoot entityToDelete);
    void Remove(TKey id);
    Task RemoveAsync(Expression<Func<TAggregateRoot, bool>>? filter, CancellationToken cancellationToken = default);
    Task RemoveAsync(TAggregateRoot entityToDelete, CancellationToken cancellationToken = default);
    Task RemoveAsync(TKey id, CancellationToken cancellationToken = default);
    IQueryable<TAggregateRoot> Query();
}

