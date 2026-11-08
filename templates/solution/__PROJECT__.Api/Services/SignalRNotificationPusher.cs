using Microsoft.AspNetCore.SignalR;

using __PROJECT__.Application.DTOs.Notification;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Api.Hubs;

namespace __PROJECT__.Api.Services;

public class SignalRNotificationPusher : INotificationPusher
{
    private readonly IHubContext<UserHub> _hub;

    public SignalRNotificationPusher(IHubContext<UserHub> hub) => _hub = hub;

    public Task PushAsync(string userId, NotificationDto notification)
        => _hub.Clients.Group(userId).SendAsync("ReceiveNotification", notification);
}
