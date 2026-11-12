using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Notification;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Application.Interfaces.Queries;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;

namespace __PROJECT__.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationQueryService _queryService;
    private readonly INotificationPusher _pusher;
    private readonly IErrorLogService _errorLog;

    public NotificationService(
        IUnitOfWork unitOfWork,
        INotificationQueryService queryService,
        INotificationPusher pusher,
        IErrorLogService errorLog)
    {
        _unitOfWork = unitOfWork;
        _queryService = queryService;
        _pusher = pusher;
        _errorLog = errorLog;
    }

    public Task<List<NotificationDto>> GetRecentAsync(string userId, int top = 20)
        => _queryService.GetRecentAsync(userId, top);

    public async Task<BaseGeneralResponse> MarkSeenAsync(string userId, long id)
    {
        var notification = await _unitOfWork.Notifications.GetByIdForUserAsync(id, userId);
        if (notification == null)
        {
            return Fail("Notification not found.");
        }

        notification.is_seen = true;
        await _unitOfWork.SaveChangesAsync();
        return Success("Notification marked as read.");
    }

    public async Task<BaseGeneralResponse> MarkAllSeenAsync(string userId)
    {
        var notifications = await _unitOfWork.Notifications.GetUnseenByUserAsync(userId);

        foreach (var notification in notifications)
        {
            notification.is_seen = true;
        }

        await _unitOfWork.SaveChangesAsync();
        return Success("Notifications marked as read.");
    }

    public async Task CreateAsync(
        string userId,
        string shortText,
        string? longText,
        string? href,
        Guid? typeId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var notification = new tb_notification
        {
            receiver_user_id = userId,
            notification_type_id = typeId,
            reff_id = href,
            is_seen = false,
            created_by = "system",
            created_date = DateTime.Now,
        };

        if (typeId == null || !await _unitOfWork.NotificationTypes.ExistsAsync(typeId.Value, cancellationToken))
        {
            var type = new tb_notification_type
            {
                id = typeId ?? Guid.NewGuid(),
                short_text = shortText,
                long_text = longText,
                href = href,
                is_send_mail = false,
                is_send_web = true,
                created_by = "system",
                created_date = DateTime.Now,
            };
            await _unitOfWork.NotificationTypes.AddAsync(type, cancellationToken);
            notification.notification_type_id = type.id;
        }
        else
        {
            // Type metadata is shared; per-notification links live in reff_id.
        }

        await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _pusher.PushAsync(userId, new NotificationDto
            {
                Id = notification.id,
                Type = shortText,
                ShortText = shortText,
                LongText = longText,
                Href = href,
                IsSeen = false,
                CreatedDate = notification.created_date,
            });
        }
        catch (Exception ex)
        {
            await _errorLog.LogAsync("Notification", "ERROR", ex);
        }
    }

    private static BaseGeneralResponse Success(string message) => new()
    {
        Success = true,
        Code = Const.SUCCESS_CODE,
        Message = message,
    };

    private static BaseGeneralResponse Fail(string message) => new()
    {
        Success = false,
        Code = Const.ERROR_CODE,
        Message = message,
    };
}
