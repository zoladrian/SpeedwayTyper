using Microsoft.JSInterop;
using NUnit.Framework;
using SpeedwayTyperApp.Client.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SpeedwayTyperApp.Client.Tests
{
    public class CustomAuthStateProviderTests
    {
        [Test]
        public async Task GetAuthenticationStateAsync_WithRoleArray_SetsAllRoles()
        {
            var jsRuntime = new TestJSRuntime();
            var httpClient = new HttpClient(new TestHttpMessageHandler());
            var provider = new CustomAuthStateProvider(jsRuntime, httpClient);
            var token = CreateToken(new[] { "Admin", "Editor" }, "roles");
            jsRuntime.SetItem("authToken", token);

            var state = await provider.GetAuthenticationStateAsync();

            Assert.Multiple(() =>
            {
                Assert.That(httpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
                Assert.That(httpClient.DefaultRequestHeaders.Authorization, Is.EqualTo(new AuthenticationHeaderValue("Bearer", token)));
                Assert.That(state.User.IsInRole("Admin"), Is.True);
                Assert.That(state.User.IsInRole("Editor"), Is.True);
            });
        }

        [Test]
        public async Task GetAuthenticationStateAsync_WithSingleRole_SetsRole()
        {
            var jsRuntime = new TestJSRuntime();
            var httpClient = new HttpClient(new TestHttpMessageHandler());
            var provider = new CustomAuthStateProvider(jsRuntime, httpClient);
            var token = CreateToken("Player", "role");
            jsRuntime.SetItem("authToken", token);

            var state = await provider.GetAuthenticationStateAsync();

            Assert.Multiple(() =>
            {
                Assert.That(httpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
                Assert.That(httpClient.DefaultRequestHeaders.Authorization, Is.EqualTo(new AuthenticationHeaderValue("Bearer", token)));
                Assert.That(state.User.IsInRole("Player"), Is.True);
            });
        }

        private static string CreateToken(object rolesValue, string rolesKey)
        {
            var payload = new Dictionary<string, object?>
            {
                [ClaimTypes.Name] = "TestUser",
                [ClaimTypes.NameIdentifier] = "123",
                ["exp"] = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds(),
                [rolesKey] = rolesValue,
            };

            return CreateToken(payload);
        }

        private static string CreateToken(Dictionary<string, object?> payload)
        {
            var headerJson = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["alg"] = "HS256",
                ["typ"] = "JWT",
            });

            var payloadJson = JsonSerializer.Serialize(payload);
            var header = Base64UrlEncode(headerJson);
            var body = Base64UrlEncode(payloadJson);

            return $"{header}.{body}.signature";
        }

        private static string Base64UrlEncode(string value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private sealed class TestJSRuntime : IJSRuntime
        {
            private readonly Dictionary<string, string?> _storage = new();

            public void SetItem(string key, string value) => _storage[key] = value;

            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
                => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            {
                switch (identifier)
                {
                    case "localStorage.getItem":
                        {
                            var key = args != null && args.Length > 0 ? args[0]?.ToString() ?? string.Empty : string.Empty;
                            if (_storage.TryGetValue(key, out var stored) && stored is not null)
                            {
                                return ValueTask.FromResult((TValue)(object)stored);
                            }

                            return ValueTask.FromResult(default(TValue)!);
                        }
                    case "localStorage.setItem":
                        {
                            var key = args != null && args.Length > 0 ? args[0]?.ToString() ?? string.Empty : string.Empty;
                            var value = args != null && args.Length > 1 ? args[1]?.ToString() : null;
                            _storage[key] = value;
                            return ValueTask.FromResult(default(TValue)!);
                        }
                    case "localStorage.removeItem":
                        {
                            var key = args != null && args.Length > 0 ? args[0]?.ToString() ?? string.Empty : string.Empty;
                            _storage.Remove(key);
                            return ValueTask.FromResult(default(TValue)!);
                        }
                    default:
                        throw new NotSupportedException($"Unsupported identifier '{identifier}'.");
                }
            }
        }

        private sealed class TestHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
