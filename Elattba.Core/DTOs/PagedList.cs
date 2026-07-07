namespace Elattba.Core.DTOs;

public sealed record PagedList<T>(
    int PageNumber,
    int PageSize,
    int Count,
    IReadOnlyList<T> Items)
    where T : class;
