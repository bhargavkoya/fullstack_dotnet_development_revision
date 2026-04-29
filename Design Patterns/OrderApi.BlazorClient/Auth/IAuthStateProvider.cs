using Microsoft.AspNetCore.Components.Authorization;

namespace OrderApi.BlazorClient.Auth;

/// <summary>[ISP] Exposes auth state operations needed by services and handlers.</summary>
public interface IAuthStateProvider
{
    Task<AuthenticationState> GetAuthenticationStateAsync();
    Task MarkUserAsAuthenticated(string accessToken);
    Task MarkUserAsLoggedOut();
}
