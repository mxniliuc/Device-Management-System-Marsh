using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeviceManagement.Models;

/// <summary>Login credentials linked to a <see cref="User"/> profile document.</summary>
[BsonIgnoreExtraElements]
public sealed class AuthUser
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("emailNormalized")]
    public string EmailNormalized { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
