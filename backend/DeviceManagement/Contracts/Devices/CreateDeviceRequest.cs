using System.ComponentModel.DataAnnotations;
using DeviceManagement.Models;
using DeviceManagement.Validation;

namespace DeviceManagement.Contracts.Devices;

public sealed record CreateDeviceRequest(
    [param: Required]
    [param: StringLength(200, MinimumLength = 1)]
    string Name,
    [param: Required]
    [param: StringLength(200, MinimumLength = 1)]
    string Manufacturer,
    [param: Required]
    [param: EnumDataType(typeof(DeviceType))]
    DeviceType Type,
    [param: Required]
    [param: StringLength(100, MinimumLength = 1)]
    string Os,
    [param: Required]
    [param: StringLength(100, MinimumLength = 1)]
    string OsVersion,
    [param: Required]
    [param: StringLength(120, MinimumLength = 1)]
    string Processor,
    [param: Range(1, 4096)]
    int RamGb,
    [param: Required]
    [param: StringLength(2000, MinimumLength = 1)]
    string Description,
    [param: Required]
    [param: StringLength(500, MinimumLength = 1)]
    string Location,
    [param: RegularExpression(ValidationPatterns.OptionalMongoObjectId, ErrorMessage = "AssignedToUserId must be empty or a 24-character hex ObjectId.")]
    string? AssignedToUserId
);
