using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace SpeedwayTyperApp.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return new AuthenticationState(_anonymous);
            }

            var claims = ParseClaimsFromJwt(token);
            var expiry = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (expiry != null)
            {
                var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry));
                if (exp.UtcDateTime <= DateTime.UtcNow)
                {
                    await MarkUserAsLoggedOut();
                    return new AuthenticationState(_anonymous);
                }
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        public async Task MarkUserAsAuthenticated(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            var user = _anonymous;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs == null)
            {
                return claims;
            }

            if (keyValuePairs.TryGetValue(ClaimTypes.Name, out var name) && name != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, name.ToString()!));
            }

            if (keyValuePairs.TryGetValue(ClaimTypes.NameIdentifier, out var nameId) && nameId != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, nameId.ToString()!));
            }

            if (keyValuePairs.TryGetValue(ClaimTypes.Role, out var roles) && roles != null)
            {
                switch (roles)
                {
                    case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Array:
                        foreach (var roleElement in jsonElement.EnumerateArray())
                        {
                            var roleValue = roleElement.GetString();
                            if (!string.IsNullOrWhiteSpace(roleValue))
                            {
                                claims.Add(new Claim(ClaimTypes.Role, roleValue));
                            }
                        }
                        break;
                    default:
                        var rolesString = roles.ToString();
                        if (!string.IsNullOrWhiteSpace(rolesString))
                        {
                            var splitRoles = rolesString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            foreach (var role in splitRoles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role));
                            }
                        }
                        break;
                }
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
