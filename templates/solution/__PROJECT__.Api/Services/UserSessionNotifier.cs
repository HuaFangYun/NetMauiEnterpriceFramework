using Microsoft.AspNetCore.SignalR;

using __PROJECT__.Api.Hubs;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Services;

public class UserSessionNotifier : IUserSessionNotifier
{
    private readonly IHubContext<UserHub> _hub;

    public UserSessionNotifier(IHubContext<UserHub> hub) => _hub = hub;

    public Task NotifySessionRevokedAsync(string userId, string? sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return Task.CompletedTask;
        }

        return _hub.Clients
            .Group($"{userId}:{sessionId}")
            .SendAsync("SessionRevoked", "This account signed in from another browser.");
    }
}
