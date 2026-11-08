using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

using __PROJECT__.Shared.Services.Interfaces;

namespace __PROJECT__.Shared.Services;

public class JwtAuthStateProvider : AuthenticationStateProvider, INotifyAuthenticationState
{
    private readonly ITokenStorage _tokenStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public JwtAuthStateProvider(ITokenStorage tokenStorage) => _tokenStorage = tokenStorage;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStorage.GetAccessTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(_anonymous);
        }

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var claims = new List<Claim>();

            foreach (var claim in jwt.Claims)
            {
                switch (claim.Type)
                {
                    case "role":
                    case ClaimTypes.Role:
                        claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                        break;
                    case "nameid":
                    case "sub":
                    case ClaimTypes.NameIdentifier:
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, claim.Value));
                        break;
                    case "unique_name":
                    case "name":
                    case ClaimTypes.Name:
                        claims.Add(new Claim(ClaimTypes.Name, claim.Value));
                        break;
                    case "email":
                    case ClaimTypes.Email:
                        claims.Add(new Claim(ClaimTypes.Email, claim.Value));
                        break;
                    default:
                        claims.Add(claim);
                        break;
                }
            }

            var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public void NotifyUserLogin(string userId)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "jwt");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
