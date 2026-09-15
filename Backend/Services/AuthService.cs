using CEP.Backend.Data;
using CEP.Backend.DTOs.Auth;
using CEP.Backend.Helpers;
using CEP.Backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CEP.Backend.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtHelper _jwtHelper;

    public AuthService(AppDbContext context, JwtHelper jwtHelper)
    {
        _context = context;
        _jwtHelper = jwtHelper;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null)
        {
            return null;
        }

        if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        var token = _jwtHelper.GenerateToken(user);

        return new LoginResponseDto
        {
            Token = token,
            Username = user.Username,
            Role = user.Role
        };
    }
}
