using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeviceManagement.Models;

[BsonIgnoreExtraElements]
public sealed class Device
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public DeviceType Type { get; set; }

    [BsonElement("os")]
    public string Os { get; set; } = string.Empty;

    [BsonElement("osVersion")]
    public string OsVersion { get; set; } = string.Empty;

    [BsonElement("processor")]
    public string Processor { get; set; } = string.Empty;

    [BsonElement("ramGb")]
    public int RamGb { get; set; }

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("location")]
    public string Location { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("assignedToUserId")]
    public string? AssignedToUserId { get; set; }
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [BsonElement("assignedAt")]
    public DateTime? AssignedAt { get; set; }
}

