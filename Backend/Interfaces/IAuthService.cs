using CEP.Backend.DTOs.Auth;

namespace CEP.Backend.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}
