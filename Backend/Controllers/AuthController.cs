using CEP.Backend.Common;
using CEP.Backend.DTOs.Auth;
using CEP.Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CEP.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Fail("Dữ liệu đăng nhập không hợp lệ."));
        }

        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            return Unauthorized(ApiResponse.Fail("Tên đăng nhập hoặc mật khẩu không chính xác."));
        }

        return Ok(result);
    }
}
