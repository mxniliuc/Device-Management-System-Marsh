using System.ComponentModel.DataAnnotations;

namespace DeviceManagement.Contracts.Users;

public sealed record CreateUserRequest(
    [param: Required]
    [param: StringLength(200, MinimumLength = 1)]
    string Name,
    [param: Required]
    [param: StringLength(100, MinimumLength = 1)]
    string Role,
    [param: Required]
    [param: StringLength(500, MinimumLength = 1)]
    string Location
);
