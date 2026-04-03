using System.ComponentModel.DataAnnotations;
using DeviceManagement.Contracts.Users;
using DeviceManagement.Models;
using DeviceManagement.Repositories;
using DeviceManagement.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManagement.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserRepository _users;

    public UsersController(IUserRepository users)
    {
        _users = users;
    }

    [HttpGet]
    public async Task<ActionResult<List<User>>> GetAll(CancellationToken ct)
    {
        return await _users.GetAllAsync(ct);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(
        [FromRoute][RegularExpression(ValidationPatterns.MongoObjectId, ErrorMessage = "Id must be a 24-character hex MongoDB ObjectId.")] string id,
        CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(id, ct);
        if (user is null) return NotFound();
        return user;
    }

    [HttpPost]
    public async Task<ActionResult<User>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var user = new User
        {
            Name = request.Name,
            Role = request.Role,
            Location = request.Location
        };

        var created = await _users.CreateAsync(user, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        [FromRoute][RegularExpression(ValidationPatterns.MongoObjectId, ErrorMessage = "Id must be a 24-character hex MongoDB ObjectId.")] string id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct)
    {
        var user = new User
        {
            Name = request.Name,
            Role = request.Role,
            Location = request.Location
        };

        var updated = await _users.UpdateAsync(id, user, ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        [FromRoute][RegularExpression(ValidationPatterns.MongoObjectId, ErrorMessage = "Id must be a 24-character hex MongoDB ObjectId.")] string id,
        CancellationToken ct)
    {
        var deleted = await _users.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
