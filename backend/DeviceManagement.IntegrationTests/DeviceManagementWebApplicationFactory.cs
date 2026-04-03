using DeviceManagement.Auth;
using DeviceManagement.MongoDb;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace DeviceManagement.IntegrationTests;

/// <summary>
/// Mongo: <c>INTEGRATION_TESTS_MONGO_CONNECTION_STRING</c>, else Docker Testcontainers, else
/// <c>mongodb://127.0.0.1:27017</c> if a ping succeeds (no Docker required).
/// </summary>
public sealed class DeviceManagementWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    internal const string IntegrationMongoEnv = "INTEGRATION_TESTS_MONGO_CONNECTION_STRING";

    private const string DefaultLocalMongo = "mongodb://127.0.0.1:27017";

    private readonly string? _externalConnectionString = Environment.GetEnvironmentVariable(IntegrationMongoEnv);
    private MongoDbContainer? _mongo;
    private string? _resolvedConnectionString;

    public async Task InitializeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_externalConnectionString))
        {
            _resolvedConnectionString = _externalConnectionString;
            return;
        }

        try
        {
            _mongo = new MongoDbBuilder("mongo:7").Build();
            await _mongo.StartAsync();
            _resolvedConnectionString = _mongo.GetConnectionString();
        }
        catch (Exception ex) when (IsDockerUnavailable(ex))
        {
            _mongo = null;
            await TryUseLocalMongoOrExplainAsync();
        }
    }

    private async Task TryUseLocalMongoOrExplainAsync()
    {
        try
        {
            using var pingCts = new CancellationTokenSource(TimeSpan.FromSeconds(4));
            var client = new MongoClient(DefaultLocalMongo);
            await client
                .GetDatabase("admin")
                .RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: pingCts.Token);
            _resolvedConnectionString = DefaultLocalMongo;
        }
        catch (Exception inner)
        {
            throw new InvalidOperationException(
                $"Integration tests need MongoDB. Docker is not available for Testcontainers. " +
                $"Start MongoDB on port 27017, start Docker, or set {IntegrationMongoEnv}. " +
                $"Underlying error: {inner.Message}",
                inner);
        }
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
            var connectionString = _resolvedConnectionString
                ?? throw new InvalidOperationException(
                    "Mongo was not initialized; IAsyncLifetime.InitializeAsync must run before the host is built.");

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{MongoDbOptions.SectionName}:ConnectionString"] = connectionString,
                [$"{MongoDbOptions.SectionName}:DatabaseName"] = "device_management_integration_tests",
                [$"{JwtOptions.SectionName}:Key"] = "test-secret-key-at-least-32-characters-long!!",
                [$"{JwtOptions.SectionName}:Issuer"] = "DeviceManagement",
                [$"{JwtOptions.SectionName}:Audience"] = "DeviceManagement",
                [$"{JwtOptions.SectionName}:ExpiresMinutes"] = "120"
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

    private static bool IsDockerUnavailable(Exception ex)
    {
        for (var e = ex; e is not null; e = e.InnerException)
        {
            if (e.GetType().Name is "DockerUnavailableException")
                return true;
            if (e is AggregateException agg)
            {
                foreach (var inner in agg.InnerExceptions)
                {
                    if (IsDockerUnavailable(inner))
                        return true;
                }
            }
        }

        return false;
    }
}
