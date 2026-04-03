using System.Net;
using System.Net.Http.Json;
using DeviceManagement.Models;
using Xunit;

namespace DeviceManagement.IntegrationTests;

[Collection("Integration")]
public sealed class UsersApiTests
{
    private readonly HttpClient _client;
    private readonly DeviceManagementWebApplicationFactory _factory;

    public UsersApiTests(DeviceManagementWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_includes_bootstrap_user_after_authentication()
    {
        await _factory.ResetDatabaseAsync();
        await AuthTestHelper.ArrangeAuthenticatedAsync(_client);

        var response = await _client.GetAsync("api/users");
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<User>>(JsonTestOptions.Default);
        Assert.NotNull(users);
        Assert.Contains(users, u => u.Name == "Integration User" && u.Role == "QA");
    }

    [Fact]
    public async Task Create_then_GetById_returns_user()
    {
        await _factory.ResetDatabaseAsync();
        await AuthTestHelper.ArrangeAuthenticatedAsync(_client);

        var create = new { name = "Alice", role = "Admin", location = "NYC" };
        var post = await _client.PostAsJsonAsync("api/users", create, JsonTestOptions.Default);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        var created = await post.Content.ReadFromJsonAsync<User>(JsonTestOptions.Default);
        Assert.NotNull(created);
        Assert.False(string.IsNullOrWhiteSpace(created.Id));

        var get = await _client.GetAsync($"api/users/{created.Id}");
        get.EnsureSuccessStatusCode();
        var roundTrip = await get.Content.ReadFromJsonAsync<User>(JsonTestOptions.Default);
        Assert.NotNull(roundTrip);
        Assert.Equal(created.Id, roundTrip.Id);
        Assert.Equal("Alice", roundTrip.Name);
        Assert.Equal("Admin", roundTrip.Role);
        Assert.Equal("NYC", roundTrip.Location);
    }

    [Fact]
    public async Task GetById_returns_not_found_for_unknown_id()
    {
        await _factory.ResetDatabaseAsync();
        await AuthTestHelper.ArrangeAuthenticatedAsync(_client);

        var response = await _client.GetAsync("api/users/507f1f77bcf86cd799439011");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_returns_no_content_and_persists_changes()
    {
        await _factory.ResetDatabaseAsync();
        await AuthTestHelper.ArrangeAuthenticatedAsync(_client);

        var create = new { name = "Bob", role = "User", location = "London" };
        var post = await _client.PostAsJsonAsync("api/users", create, JsonTestOptions.Default);
        var created = await post.Content.ReadFromJsonAsync<User>(JsonTestOptions.Default);
        Assert.NotNull(created);

        var update = new { name = "Robert", role = "User", location = "Paris" };
        var put = await _client.PutAsJsonAsync($"api/users/{created.Id}", update, JsonTestOptions.Default);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var get = await _client.GetAsync($"api/users/{created.Id}");
        get.EnsureSuccessStatusCode();
        var user = await get.Content.ReadFromJsonAsync<User>(JsonTestOptions.Default);
        Assert.NotNull(user);
        Assert.Equal("Robert", user.Name);
        Assert.Equal("Paris", user.Location);
    }

    [Fact]
    public async Task Delete_returns_no_content_and_removes_user()
    {
        await _factory.ResetDatabaseAsync();
        await AuthTestHelper.ArrangeAuthenticatedAsync(_client);

        var create = new { name = "Carol", role = "User", location = "Berlin" };
        var post = await _client.PostAsJsonAsync("api/users", create, JsonTestOptions.Default);
        var created = await post.Content.ReadFromJsonAsync<User>(JsonTestOptions.Default);
        Assert.NotNull(created);

        var del = await _client.DeleteAsync($"api/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        var get = await _client.GetAsync($"api/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
