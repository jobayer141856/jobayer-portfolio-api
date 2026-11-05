namespace Portfolio.Domain.Contracts;

public interface IUnitOfWork
{
    void Save();
    Task SaveAsync(CancellationToken cancellationToken = default);
}

