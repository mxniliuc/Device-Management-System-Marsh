using System.Net.Http.Headers;
using System.Net.Http.Json;
using DeviceManagement.Contracts.Auth;
using Xunit;

namespace DeviceManagement.IntegrationTests;

/// <summary>Registers a throwaway account and attaches Bearer — only for bootstrapping protected API tests.</summary>
internal static class AuthTestHelper
{
    public static async Task ArrangeAuthenticatedAsync(HttpClient client)
    {
        var token = await RegisterAndGetTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<string> RegisterAndGetTokenAsync(HttpClient client)
    {
        var email = $"{Guid.NewGuid():N}@test.local";
        const string password = "TestPassword123!";
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                email,
                password,
                confirmPassword = password,
                name = "Integration User",
                role = "QA",
                location = "Test location",
            });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonTestOptions.Default);
        Assert.NotNull(body);
        return body.Token;
    }
}
