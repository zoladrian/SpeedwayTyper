using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace SpeedwayTyperApp.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (string.IsNullOrEmpty(token))
            {
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

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        public async Task MarkUserAsAuthenticated(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            var user = _anonymous;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            keyValuePairs.TryGetValue(ClaimTypes.Name, out object name);
            keyValuePairs.TryGetValue(ClaimTypes.NameIdentifier, out object nameId);
            keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

            if (name != null) claims.Add(new Claim(ClaimTypes.Name, name.ToString()));
            if (nameId != null) claims.Add(new Claim(ClaimTypes.NameIdentifier, nameId.ToString()));
            if (roles != null)
            {
                if (roles is JsonElement rolesElement)
                {
                    if (rolesElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in rolesElement.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            if (!string.IsNullOrWhiteSpace(roleValue))
                            {
                                claims.Add(new Claim(ClaimTypes.Role, roleValue));
                            }
                        }
                    }
                    else
                    {
                        var singleRole = rolesElement.GetString();
                        if (!string.IsNullOrWhiteSpace(singleRole))
                        {
                            claims.Add(new Claim(ClaimTypes.Role, singleRole));
                        }
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
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