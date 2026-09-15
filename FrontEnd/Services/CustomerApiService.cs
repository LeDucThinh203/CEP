using System.Net.Http.Json;
using System.Text.Json;
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

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
                return (true, result?.Message ?? "Thêm khách hàng thành công.", result?.Data);
            }

            var errorMessage = await ParseErrorMessageAsync(response);
            return (false, errorMessage, null);
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

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (true, result?.Message ?? "Cập nhật khách hàng thành công.");
            }

            var errorMessage = await ParseErrorMessageAsync(response);
            return (false, errorMessage);
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

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (true, result?.Message ?? "Xóa khách hàng thành công.");
            }

            var errorMessage = await ParseErrorMessageAsync(response);
            return (false, errorMessage);
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi kết nối: {ex.Message}");
        }
    }

    private static async Task<string> ParseErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var rawContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(rawContent))
            {
                return $"Yêu cầu thất bại với mã lỗi HTTP {(int)response.StatusCode}.";
            }

            using var doc = JsonDocument.Parse(rawContent);
            var root = doc.RootElement;

            // 1. Check custom ApiResponse message
            if (root.TryGetProperty("message", out var messageProp) && !string.IsNullOrWhiteSpace(messageProp.GetString()))
            {
                return messageProp.GetString()!;
            }

            // 2. Check ASP.NET Core ValidationProblemDetails errors dictionary/array
            if (root.TryGetProperty("errors", out var errorsProp))
            {
                var errorList = new List<string>();
                if (errorsProp.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in errorsProp.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var err in prop.Value.EnumerateArray())
                            {
                                var text = err.GetString();
                                if (!string.IsNullOrWhiteSpace(text)) errorList.Add(text);
                            }
                        }
                    }
                }
                else if (errorsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var err in errorsProp.EnumerateArray())
                    {
                        var text = err.GetString();
                        if (!string.IsNullOrWhiteSpace(text)) errorList.Add(text);
                    }
                }

                if (errorList.Count > 0)
                {
                    return string.Join(" ", errorList);
                }
            }

            if (root.TryGetProperty("title", out var titleProp) && !string.IsNullOrWhiteSpace(titleProp.GetString()))
            {
                return titleProp.GetString()!;
            }

            return rawContent;
        }
        catch
        {
            return $"Lỗi xử lý phản hồi từ máy chủ (Mã {(int)response.StatusCode}).";
        }
    }
}
