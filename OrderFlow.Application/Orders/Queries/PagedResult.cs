namespace OrderFlow.Application.Orders.Queries;
    public record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

