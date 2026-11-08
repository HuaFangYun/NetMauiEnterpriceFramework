using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
        => _notificationService = notificationService;

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int top = 20)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrWhiteSpace(userId)
            ? Unauthorized()
            : Ok(await _notificationService.GetRecentAsync(userId, top));
    }

    [HttpPost("{id:long}/seen")]
    public async Task<IActionResult> MarkSeen(long id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var result = await _notificationService.MarkSeenAsync(userId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("seen-all")]
    public async Task<IActionResult> MarkAllSeen()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var result = await _notificationService.MarkAllSeenAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
