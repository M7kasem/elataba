namespace Elattba.Core.DTOs;

public sealed class StoreParams
{
    public const int MaxPageSize = 50;

    private int _pageNumber = 1;
    private int _pageSize = 10;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value <= 0 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? 10 : Math.Min(value, MaxPageSize);
    }

    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? Location { get; set; }
}
