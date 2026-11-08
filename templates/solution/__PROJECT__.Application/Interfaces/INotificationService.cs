using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Notification;

namespace __PROJECT__.Application.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetRecentAsync(string userId, int top = 20);
    Task<BaseGeneralResponse> MarkSeenAsync(string userId, long id);
    Task<BaseGeneralResponse> MarkAllSeenAsync(string userId);
    Task CreateAsync(
        string userId,
        string shortText,
        string? longText,
        string? href,
        Guid? typeId = null,
        CancellationToken cancellationToken = default);
}

public interface INotificationPusher
{
    Task PushAsync(string userId, NotificationDto notification);
}
