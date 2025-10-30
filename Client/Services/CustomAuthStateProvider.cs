using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

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

            var name = GetSingleClaimValue(keyValuePairs, ClaimTypes.Name, JwtRegisteredClaimNames.UniqueName);
            if (!string.IsNullOrWhiteSpace(name))
            {
                claims.Add(new Claim(ClaimTypes.Name, name));
            }

            var nameId = GetSingleClaimValue(keyValuePairs, ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.Sub);
            if (!string.IsNullOrWhiteSpace(nameId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, nameId));
            }

            var roles = GetClaimValue(keyValuePairs, ClaimTypes.Role, "role", "roles", JwtRegisteredClaimNames.Typ);

            foreach (var role in GetRoleValues(roles))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private static object? GetClaimValue(Dictionary<string, object> keyValuePairs, params string[] claimKeys)
        {
            foreach (var key in claimKeys)
            {
                if (keyValuePairs.TryGetValue(key, out var value) && value != null)
                {
                    return value;
                }
            }

            return null;
        }

        private static string? GetSingleClaimValue(Dictionary<string, object> keyValuePairs, params string[] claimKeys)
        {
            foreach (var key in claimKeys)
            {
                if (keyValuePairs.TryGetValue(key, out var value) && value != null)
                {
                    var stringValue = ConvertJsonElementToString(value);
                    if (!string.IsNullOrWhiteSpace(stringValue))
                    {
                        return stringValue;
                    }
                }
            }

            return null;
        }

        private static IEnumerable<string> GetRoleValues(object? roles)
        {
            if (roles == null)
            {
                yield break;
            }

            switch (roles)
            {
                case JsonElement element when element.ValueKind == JsonValueKind.Array:
                    foreach (var jsonRole in element.EnumerateArray())
                    {
                        var role = ConvertJsonElementToString(jsonRole);
                        if (!string.IsNullOrWhiteSpace(role))
                        {
                            yield return role;
                        }
                    }
                    yield break;
                case JsonElement element:
                    {
                        var role = ConvertJsonElementToString(element);
                        if (!string.IsNullOrWhiteSpace(role))
                        {
                            yield return role;
                        }
                        yield break;
                    }
                default:
                    var roleValue = roles.ToString();
                    if (!string.IsNullOrWhiteSpace(roleValue))
                    {
                        yield return roleValue;
                    }
                    yield break;
            }
        }

        private static string? ConvertJsonElementToString(object value)
        {
            if (value is JsonElement element)
            {
                return element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),
                    JsonValueKind.Number => element.ToString(),
                    JsonValueKind.True => bool.TrueString,
                    JsonValueKind.False => bool.FalseString,
                    JsonValueKind.Null => null,
                    JsonValueKind.Undefined => null,
                    _ => element.ToString(),
                };
            }

            return value?.ToString();
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
