using System.Net.Http.Json;
using CEP.FrontEnd.Models;

namespace CEP.FrontEnd.Services;

public class CustomerApiService
{
    private readonly HttpClient _httpClient;

    public CustomerApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<PagedResult<CustomerDto>>?> GetCustomersAsync(
        string? search = null,
        bool? isActive = null,
        string? sortBy = null,
        bool sortDescending = false,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                queryParams.Add($"search={Uri.EscapeDataString(search.Trim())}");
            }

            if (isActive.HasValue)
            {
                queryParams.Add($"isActive={isActive.Value}");
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
                queryParams.Add($"sortDescending={sortDescending}");
            }

            var url = $"api/customers?{string.Join("&", queryParams)}";
            return await _httpClient.GetFromJsonAsync<ApiResponse<PagedResult<CustomerDto>>>(url);
        }
        catch (Exception)
        {
            return new ApiResponse<PagedResult<CustomerDto>>
            {
                Success = false,
                Message = "Không thể tải danh sách khách hàng từ máy chủ."
            };
        }
    }

    public async Task<ApiResponse<CustomerDto>?> GetCustomerByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ApiResponse<CustomerDto>>($"api/customers/{id}");
        }
        catch (Exception)
        {
            return new ApiResponse<CustomerDto>
            {
                Success = false,
                Message = $"Không thể tải thông tin khách hàng ID = {id}."
            };
        }
    }

    public async Task<(bool Success, string Message, CustomerDto? Data)> CreateCustomerAsync(CreateCustomerDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/customers", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();

            if (response.IsSuccessStatusCode)
            {
                return (true, result?.Message ?? "Thêm khách hàng thành công.", result?.Data);
            }

            return (false, result?.Message ?? "Không thể thêm khách hàng.", null);
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi kết nối: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateCustomerAsync(int id, UpdateCustomerDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/customers/{id}", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();

            if (response.IsSuccessStatusCode)
            {
                return (true, result?.Message ?? "Cập nhật khách hàng thành công.");
            }

            return (false, result?.Message ?? "Không thể cập nhật khách hàng.");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi kết nối: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteCustomerAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/customers/{id}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();

            if (response.IsSuccessStatusCode)
            {
                return (true, result?.Message ?? "Xóa khách hàng thành công.");
            }

            return (false, result?.Message ?? "Không thể xóa khách hàng.");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi kết nối: {ex.Message}");
        }
    }
}
