using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace DeviceManagement.IntegrationTests;

[Collection("Integration")]
public sealed class ExceptionHandlingIntegrationTests
{
    private readonly HttpClient _client;
    private readonly DeviceManagementWebApplicationFactory _factory;

    public ExceptionHandlingIntegrationTests(DeviceManagementWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Invalid_json_returns_400_problem_json()
    {
        await _factory.ResetDatabaseAsync();

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/users")
        {
            Content = new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonTestOptions.Default);
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.True(problem.TryGetProperty("title", out var title));
        Assert.False(string.IsNullOrWhiteSpace(title.GetString()));
    }
}
