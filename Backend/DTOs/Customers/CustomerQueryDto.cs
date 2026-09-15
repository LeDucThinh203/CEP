namespace CEP.Backend.DTOs.Customers;

public class CustomerQueryDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } // CustomerCode, FullName, CreatedAt
    public bool SortDescending { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
