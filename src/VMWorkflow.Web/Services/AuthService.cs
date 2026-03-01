using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using VMWorkflow.Web.Models;

namespace VMWorkflow.Web.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient http, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResponse?> LoginAsync(LoginModel model)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", model);
        if (!response.IsSuccessStatusCode)
            return null;

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth != null)
        {
            await _localStorage.SetItemAsStringAsync("authToken", auth.Token);
            ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(auth.Token);
        }

        return auth;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterModel model)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", model);
        if (!response.IsSuccessStatusCode)
            return null;

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth != null)
        {
            await _localStorage.SetItemAsStringAsync("authToken", auth.Token);
            ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(auth.Token);
        }

        return auth;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
    }
}
