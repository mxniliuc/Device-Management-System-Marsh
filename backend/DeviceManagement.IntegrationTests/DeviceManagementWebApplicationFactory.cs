using DeviceManagement.MongoDb;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace DeviceManagement.IntegrationTests;

/// <summary>
/// Boots the API with a real MongoDB. Prefers Testcontainers when Docker is available;
/// set INTEGRATION_TESTS_MONGO_CONNECTION_STRING (e.g. mongodb://localhost:27017) to use a local instance instead.
/// </summary>
public sealed class DeviceManagementWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string IntegrationMongoEnv = "INTEGRATION_TESTS_MONGO_CONNECTION_STRING";

    private readonly string? _externalConnectionString = Environment.GetEnvironmentVariable(IntegrationMongoEnv);
    private MongoDbContainer? _mongo;

    public async Task InitializeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_externalConnectionString))
            return;

        _mongo = new MongoDbBuilder("mongo:7").Build();
        await _mongo.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        if (_mongo is not null)
            await _mongo.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var connectionString = !string.IsNullOrWhiteSpace(_externalConnectionString)
                ? _externalConnectionString
                : _mongo!.GetConnectionString();

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{MongoDbOptions.SectionName}:ConnectionString"] = connectionString,
                [$"{MongoDbOptions.SectionName}:DatabaseName"] = "device_management_integration_tests"
            });
        });
    }

    public async Task ResetDatabaseAsync(CancellationToken ct = default)
    {
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        var names = await ctx.Database.ListCollectionNames().ToListAsync(ct);
        foreach (var name in names)
            await ctx.Database.DropCollectionAsync(name, ct);
    }
}
