namespace Application;
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);