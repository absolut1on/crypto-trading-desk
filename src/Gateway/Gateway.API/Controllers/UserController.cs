using System.Security.Claims;
using Gateway.API.Data;
using Gateway.API.Entities;
using Gateway.API.StateMachines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly AppDbContext _db;

    public UserController(AppDbContext db) => _db = db;

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (sub is null || !Guid.TryParse(sub, out var userId))
            return Unauthorized();

        var user = await _db.Users.FindAsync(userId);
        if (user is null)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? $"{userId}@supabase";
            user = new User { Id = userId, Email = email, SupabaseId = userId.ToString(), Status = UserStatus.Active };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        return Ok(user);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("{id:guid}/kyc")]
    public async Task<IActionResult> SubmitKyc(Guid id, [FromBody] SubmitKycRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        if (!UserStateMachine.CanTransition(user.Status, "SubmitKyc"))
            return BadRequest($"Cannot submit KYC from status {user.Status}");

        user.Status = UserStateMachine.Transition(user.Status, "SubmitKyc");
        user.KycData = request.KycData;
        user.UpdatedAt = DateTime.UtcNow;

        _db.UserEvents.Add(new UserEvent
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EventType = "SubmitKyc",
            Payload = request.KycData
        });

        await _db.SaveChangesAsync();
        return Ok(user);
    }
}

public record SubmitKycRequest(string KycData);
