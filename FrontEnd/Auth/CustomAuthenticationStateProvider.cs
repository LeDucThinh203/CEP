using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using CEP.FrontEnd.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace CEP.FrontEnd.Auth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly TokenService _tokenService;
    private readonly AuthenticationState _anonymous;

    public CustomAuthenticationStateProvider(TokenService tokenService)
    {
        _tokenService = tokenService;
        _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return _anonymous;
        }

        var claims = ParseClaimsFromJwt(token);
        if (claims == null || !claims.Any())
        {
            await _tokenService.RemoveTokenAsync();
            return _anonymous;
        }

        // Check token expiration
        var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
        if (expClaim != null && long.TryParse(expClaim.Value, out var expSeconds))
        {
            var expDate = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
            if (expDate <= DateTimeOffset.UtcNow)
            {
                await _tokenService.RemoveTokenAsync();
                return _anonymous;
            }
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public void MarkUserAsAuthenticated(string token)
    {
        var claims = ParseClaimsFromJwt(token) ?? Enumerable.Empty<Claim>();
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void MarkUserAsLoggedOut()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }

    private static IEnumerable<Claim>? ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return null;

            var payload = parts[1];
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs == null) return null;

            var claims = new List<Claim>();
            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        claims.Add(new Claim(kvp.Key, item.ToString()));
                    }
                }
                else
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty));
                }
            }

            return claims;
        }
        catch
        {
            return null;
        }
    }
}
