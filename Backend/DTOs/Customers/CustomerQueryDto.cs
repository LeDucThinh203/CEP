namespace CEP.Backend.DTOs.Customers;

public class CustomerQueryDto
{
    public const int MaxPageSize = 100;
    public const int DefaultPageSize = 10;
    private int _pageSize = DefaultPageSize;

    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } // CustomerCode, FullName, CreatedAt
    public bool SortDescending { get; set; } = false;
    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value <= 0 ? DefaultPageSize : value, 1, MaxPageSize);
    }
}
