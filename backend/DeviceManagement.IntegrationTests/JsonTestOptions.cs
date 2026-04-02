using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeviceManagement.IntegrationTests;

internal static class JsonTestOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}
