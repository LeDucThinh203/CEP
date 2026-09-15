using System.Net;
using System.Net.Http.Headers;
using CEP.FrontEnd.Services;
using Microsoft.AspNetCore.Components;

namespace CEP.FrontEnd.Auth;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly TokenService _tokenService;
    private readonly NavigationManager _navigationManager;

    public AuthorizationMessageHandler(TokenService tokenService, NavigationManager navigationManager)
    {
        _tokenService = tokenService;
        _navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _tokenService.RemoveTokenAsync();
            _navigationManager.NavigateTo("/login", forceLoad: false);
        }

        return response;
    }
}
