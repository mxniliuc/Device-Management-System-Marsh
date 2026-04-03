using System.ComponentModel.DataAnnotations;
using DeviceManagement.Models;

namespace DeviceManagement.Contracts.Devices;

/// <summary>Technical specs used to produce an AI-written inventory description (no location / assignment).</summary>
public sealed record GenerateDeviceDescriptionRequest(
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
    int RamGb
);
