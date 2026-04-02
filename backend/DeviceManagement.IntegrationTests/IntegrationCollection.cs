using Xunit;

namespace DeviceManagement.IntegrationTests;

[CollectionDefinition("Integration", DisableParallelization = true)]
public sealed class IntegrationCollection : ICollectionFixture<DeviceManagementWebApplicationFactory>
{
}
