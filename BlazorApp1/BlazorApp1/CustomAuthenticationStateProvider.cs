using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BlazorApp1;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSessionStorageResult = await _sessionStorage.GetAsync<UserSession>("UserSession");
            var userSession = userSessionStorageResult.Value ?? null;
            if (userSession == null) return await Task.FromResult(new AuthenticationState(_anonymous));

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.Name, userSession.UserName),
                new(ClaimTypes.Role, userSession.Role)
            }, "CustomAuth"));

            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task UpdateAuthenticationState(UserSession? userSession)
    {
        //New a Claim to Authenticate
        ClaimsPrincipal claimsPrincipal;

        if (userSession != null)
        {
            //Set SessionStorage
            await _sessionStorage.SetAsync("UserSession", userSession);
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.Name, userSession.UserName),
                new(ClaimTypes.Role, userSession.Role)
            }));
        }
        else
        {
            await _sessionStorage.DeleteAsync("UserSession");
            claimsPrincipal = _anonymous;
        }

        //Do AuthenticationTask
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }
}