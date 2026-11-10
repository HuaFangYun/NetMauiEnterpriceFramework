using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace __PROJECT__.Api.Hubs;

[Authorize]
public class UserHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            var sessionId = Context.User?.FindFirst("session_id")?.Value;
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"{userId}:{sessionId}");
            }
        }

        await base.OnConnectedAsync();
    }
}
