namespace Portfolio.Domain.Contracts;

public interface IAggregateRoot<TKey> where TKey : IComparable
{
    TKey Id { get; set; }
}

