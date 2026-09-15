using CEP.Backend.Common;
using CEP.Backend.DTOs.Customers;

namespace CEP.Backend.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetCustomersAsync(CustomerQueryDto queryDto);
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto, string? currentUsername = null);
    Task<bool> UpdateAsync(int id, UpdateCustomerDto dto, string? currentUsername = null);
    Task<bool> DeleteAsync(int id, string? currentUsername = null);
}
