using System.Net;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Components;

using __PROJECT__.Shared.Services.Interfaces;

namespace __PROJECT__.Shared.Handlers;

public class AuthenticatingHttpHandler : DelegatingHandler
{
    private readonly ITokenStorage _tokenStorage;
    private readonly NavigationManager _navigation;

    public AuthenticatingHttpHandler(ITokenStorage tokenStorage, NavigationManager navigation)
    {
        _tokenStorage = tokenStorage;
        _navigation = navigation;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _tokenStorage.RemoveTokensAsync();
            if (!_navigation.Uri.EndsWith('/'))
            {
                _navigation.NavigateTo("/", true);
            }
        }

        return response;
    }
}
