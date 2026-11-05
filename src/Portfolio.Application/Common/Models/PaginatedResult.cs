namespace Portfolio.Application.Common.Models;

public class PaginatedResult<T>
{
    public IList<T> Items { get; init; } = new List<T>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedResult<T> Create(IList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PaginatedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}

