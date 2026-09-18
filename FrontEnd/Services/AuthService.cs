using System.Net.Http.Json;
using CEP.FrontEnd.Auth;
using CEP.FrontEnd.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace CEP.FrontEnd.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(
        HttpClient httpClient,
        TokenService tokenService,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _authStateProvider = authStateProvider;
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (result != null && !string.IsNullOrWhiteSpace(result.Token))
                {
                    await _tokenService.SetTokenAsync(result.Token);
                    if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
                    {
                        customProvider.MarkUserAsAuthenticated(result.Token);
                    }

                    return (true, null);
                }
            }

            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (!string.IsNullOrWhiteSpace(errorResponse?.Message))
                {
                    return (false, errorResponse.Message);
                }
            }
            catch
            {
                // Response content was not JSON or did not match ApiResponse
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (false, "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            return (false, "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.");
        }
        catch (Exception ex)
        {
            return (false, $"Không thể kết nối tới máy chủ: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        await _tokenService.RemoveTokenAsync();
        // Xóa Token khỏi localStorage
        if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
        {
            customProvider.MarkUserAsLoggedOut();
        }
    }
}
