using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DeviceManagement.Contracts.Devices;
using DeviceManagement.Models;
using DeviceManagement.Repositories;
using DeviceManagement.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManagement.Controllers;

[ApiController]
[Authorize]
[Route("api/devices")]
public sealed class DevicesController : ControllerBase
{
    private readonly IDeviceRepository _devices;

    public DevicesController(IDeviceRepository devices)
    {
        _devices = devices;
    }

    [HttpGet]
    public async Task<ActionResult<List<Device>>> GetAll(CancellationToken ct)
    {
        return await _devices.GetAllAsync(ct);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Device>> GetById(
        [FromRoute][RegularExpression(ValidationPatterns.MongoObjectId, ErrorMessage = "Id must be a 24-character hex MongoDB ObjectId.")] string id,
        CancellationToken ct)
    {
        var device = await _devices.GetByIdAsync(id, ct);
        if (device is null) return NotFound();
        return device;
    }

    [HttpPost]
    public async Task<ActionResult<Device>> Create([FromBody] CreateDeviceRequest request, CancellationToken ct)
    {
        var device = new Device
        {
            Name = request.Name,
            Manufacturer = request.Manufacturer,
            Type = request.Type,
            Os = request.Os,
            OsVersion = request.OsVersion,
            Processor = request.Processor,
            RamGb = request.RamGb,
            Description = request.Description,
            Location = request.Location,
            AssignedToUserId = request.AssignedToUserId
        };

        var created = await _devices.CreateAsync(device, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        [FromRoute][RegularExpression(ValidationPatterns.MongoObjectId, ErrorMessage = "Id must be a 24-character hex MongoDB ObjectId.")] string id,
        [FromBody] UpdateDeviceRequest request,
        CancellationToken ct)
    {
        var device = new Device
        {
            Name = request.Name,
            Manufacturer = request.Manufacturer,
            Type = request.Type,
            Os = request.Os,
            OsVersion = request.OsVersion,
            Processor = request.Processor,
            RamGb = request.RamGb,
            Description = request.Description,
            Location = request.Location,
            AssignedToUserId = request.AssignedToUserId
        };

        var updated = await _devices.UpdateAsync(id, device, ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        [FromRoute][RegularExpression(ValidationPatterns.MongoObjectId, ErrorMessage = "Id must be a 24-character hex MongoDB ObjectId.")] string id,
        CancellationToken ct)
    {
        var deleted = await _devices.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Assigns the device to the signed-in user if it is currently unassigned.</summary>
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> AssignToSelf(
        [FromRoute][RegularExpression(ValidationPatterns.MongoObjectId, ErrorMessage = "Id must be a 24-character hex MongoDB ObjectId.")] string id,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var device = await _devices.GetByIdAsync(id, ct);
        if (device is null)
            return NotFound();

        if (device.AssignedToUserId is not null && device.AssignedToUserId != userId)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = "This device is already assigned to someone else."
            });
        }

        device.AssignedToUserId = userId;
        device.AssignedAt = DateTime.UtcNow;
        var updated = await _devices.UpdateAsync(id, device, ct);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Removes your assignment from this device.</summary>
    [HttpPost("{id}/unassign")]
    public async Task<IActionResult> UnassignSelf(
        [FromRoute][RegularExpression(ValidationPatterns.MongoObjectId, ErrorMessage = "Id must be a 24-character hex MongoDB ObjectId.")] string id,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var device = await _devices.GetByIdAsync(id, ct);
        if (device is null)
            return NotFound();

        if (device.AssignedToUserId != userId)
            return Forbid();

        device.AssignedToUserId = null;
        device.AssignedAt = null;
        var updated = await _devices.UpdateAsync(id, device, ct);
        return updated ? NoContent() : NotFound();
    }
}
