using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using LastClue.Application.Rooms;
using LastClue.Web.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LastClue.Web.Controllers;

[Authorize(Policy = "PlayerOnly")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class RoomsController(IRoomService rooms) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var room = await rooms.CreateAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, request.Language,
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, room);
    }

    [HttpGet("{roomId:guid}")]
    public async Task<IActionResult> Get(Guid roomId, CancellationToken cancellationToken) =>
        Ok(await rooms.GetAsync(roomId, User.FindFirstValue(ClaimTypes.NameIdentifier)!, cancellationToken));

    [HttpPut("{roomId:guid}/ready")]
    public async Task<IActionResult> SetReady(Guid roomId, ReadyRequest request, CancellationToken cancellationToken) =>
        Ok(await rooms.SetReadyAsync(roomId, User.FindFirstValue(ClaimTypes.NameIdentifier)!, request.IsReady!.Value,
            cancellationToken));

    [HttpPost("{roomId:guid}/leave")]
    public async Task<IActionResult> Leave(Guid roomId, CancellationToken cancellationToken)
    {
        await rooms.LeaveAsync(roomId, User.FindFirstValue(ClaimTypes.NameIdentifier)!, cancellationToken);
        return NoContent();
    }

    [HttpPost("join")]
    [EnableRateLimiting(RoomRateLimits.Entry)]
    public async Task<IActionResult> Join(JoinRoomRequest request, CancellationToken cancellationToken)
    {
        var room = await rooms.JoinAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, request.Code,
            cancellationToken);
        return Ok(room);
    }
}

public sealed record CreateRoomRequest([Required, StringLength(2)] string Language);
public sealed record JoinRoomRequest([Required, StringLength(32)] string Code);
public sealed record ReadyRequest([Required] bool? IsReady);
