using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace VMWorkflow.Web.Services;

public class AuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public AuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("authToken");

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(_anonymous);

        var claims = ParseClaimsFromJwt(token);
        if (claims == null)
            return new AuthenticationState(_anonymous);

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    private static IEnumerable<Claim>? ParseClaimsFromJwt(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Map standard JWT claims to ClaimTypes for Blazor auth
            var claims = new List<Claim>(jwt.Claims);

            var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)
                         ?? jwt.Claims.FirstOrDefault(c => c.Type == "role");
            if (roleClaim != null && !claims.Any(c => c.Type == ClaimTypes.Role))
                claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));

            var nameClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)
                         ?? jwt.Claims.FirstOrDefault(c => c.Type == "name");
            if (nameClaim != null && !claims.Any(c => c.Type == ClaimTypes.Name))
                claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));

            return claims;
        }
        catch
        {
            return null;
        }
    }
}
