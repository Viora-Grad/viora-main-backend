namespace Viora.Application.Abstractions.Pagination;

public class PaginatedModel<TValue>
{
    public IReadOnlyList<TValue> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public long TotalCount { get; }

    public int Count => Items.Count;
    public int TotalPages => PageSize == 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public int? NextPage => HasNextPage ? Page + 1 : null;
    public int? PreviousPage => HasPreviousPage ? Page - 1 : null;

    public PaginatedModel(IEnumerable<TValue> items, int page, int pageSize, long totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        Items = items as IReadOnlyList<TValue> ?? [.. items];
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PaginatedModel<TValue> Empty(int page, int pageSize)
        => new([], page, pageSize, 0);
}