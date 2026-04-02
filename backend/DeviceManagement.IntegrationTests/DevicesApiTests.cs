using System.Net;
using System.Net.Http.Json;
using DeviceManagement.Models;
using Xunit;

namespace DeviceManagement.IntegrationTests;

[Collection("Integration")]
public sealed class DevicesApiTests
{
    private readonly HttpClient _client;
    private readonly DeviceManagementWebApplicationFactory _factory;

    public DevicesApiTests(DeviceManagementWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_returns_empty_list_when_database_is_empty()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.GetAsync("api/devices");
        response.EnsureSuccessStatusCode();

        var devices = await response.Content.ReadFromJsonAsync<List<Device>>(JsonTestOptions.Default);
        Assert.NotNull(devices);
        Assert.Empty(devices);
    }

    [Fact]
    public async Task Create_then_GetById_returns_device()
    {
        await _factory.ResetDatabaseAsync();

        var body = new
        {
            name = "Pixel",
            manufacturer = "Google",
            type = "Phone",
            os = "Android",
            osVersion = "15",
            processor = "Tensor",
            ramGb = 8,
            description = "Test phone",
            location = "Lab",
            assignedToUserId = (string?)null
        };

        var post = await _client.PostAsJsonAsync("api/devices", body, JsonTestOptions.Default);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        var created = await post.Content.ReadFromJsonAsync<Device>(JsonTestOptions.Default);
        Assert.NotNull(created);
        Assert.False(string.IsNullOrWhiteSpace(created.Id));

        var get = await _client.GetAsync($"api/devices/{created.Id}");
        get.EnsureSuccessStatusCode();
        var device = await get.Content.ReadFromJsonAsync<Device>(JsonTestOptions.Default);
        Assert.NotNull(device);
        Assert.Equal(created.Id, device.Id);
        Assert.Equal("Pixel", device.Name);
        Assert.Equal(DeviceType.Phone, device.Type);
    }

    [Fact]
    public async Task Create_with_assigned_user_persists_assignment()
    {
        await _factory.ResetDatabaseAsync();

        var userPost = await _client.PostAsJsonAsync(
            "api/users",
            new { name = "Owner", role = "Admin", location = "HQ" },
            JsonTestOptions.Default);
        var user = await userPost.Content.ReadFromJsonAsync<User>(JsonTestOptions.Default);
        Assert.NotNull(user);

        var body = new
        {
            name = "iPad",
            manufacturer = "Apple",
            type = "Tablet",
            os = "iPadOS",
            osVersion = "18",
            processor = "M4",
            ramGb = 16,
            description = "Tablet",
            location = "HQ",
            assignedToUserId = user.Id
        };

        var post = await _client.PostAsJsonAsync("api/devices", body, JsonTestOptions.Default);
        post.EnsureSuccessStatusCode();
        var created = await post.Content.ReadFromJsonAsync<Device>(JsonTestOptions.Default);
        Assert.NotNull(created);
        Assert.Equal(user.Id, created.AssignedToUserId);
    }

    [Fact]
    public async Task GetById_returns_not_found_for_unknown_id()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.GetAsync("api/devices/507f1f77bcf86cd799439011");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_returns_no_content_and_persists_changes()
    {
        await _factory.ResetDatabaseAsync();

        var body = new
        {
            name = "Surface",
            manufacturer = "Microsoft",
            type = "Tablet",
            os = "Windows",
            osVersion = "11",
            processor = "SQ3",
            ramGb = 16,
            description = "2-in-1",
            location = "Seattle",
            assignedToUserId = (string?)null
        };

        var post = await _client.PostAsJsonAsync("api/devices", body, JsonTestOptions.Default);
        var created = await post.Content.ReadFromJsonAsync<Device>(JsonTestOptions.Default);
        Assert.NotNull(created);

        var update = new
        {
            name = "Surface Pro",
            manufacturer = "Microsoft",
            type = "Tablet",
            os = "Windows",
            osVersion = "11",
            processor = "SQ3",
            ramGb = 16,
            description = "2-in-1",
            location = "Redmond",
            assignedToUserId = (string?)null
        };

        var put = await _client.PutAsJsonAsync($"api/devices/{created.Id}", update, JsonTestOptions.Default);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var get = await _client.GetAsync($"api/devices/{created.Id}");
        get.EnsureSuccessStatusCode();
        var device = await get.Content.ReadFromJsonAsync<Device>(JsonTestOptions.Default);
        Assert.NotNull(device);
        Assert.Equal("Surface Pro", device.Name);
        Assert.Equal("Redmond", device.Location);
    }

    [Fact]
    public async Task Delete_returns_no_content_and_removes_device()
    {
        await _factory.ResetDatabaseAsync();

        var body = new
        {
            name = "Kindle",
            manufacturer = "Amazon",
            type = "Tablet",
            os = "Kindle",
            osVersion = "5",
            processor = "MediaTek",
            ramGb = 2,
            description = "Reader",
            location = "Shelf",
            assignedToUserId = (string?)null
        };

        var post = await _client.PostAsJsonAsync("api/devices", body, JsonTestOptions.Default);
        var created = await post.Content.ReadFromJsonAsync<Device>(JsonTestOptions.Default);
        Assert.NotNull(created);

        var del = await _client.DeleteAsync($"api/devices/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        var get = await _client.GetAsync($"api/devices/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
