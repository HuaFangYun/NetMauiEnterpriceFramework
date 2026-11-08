using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Notification;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly __PROJECT__DbContext _context;
    private readonly INotificationPusher _pusher;

    public NotificationService(__PROJECT__DbContext context, INotificationPusher pusher)
    {
        _context = context;
        _pusher = pusher;
    }

    public async Task<List<NotificationDto>> GetRecentAsync(string userId, int top = 20)
    {
        var notifications = await (from notification in _context.tb_notification
                                   join type in _context.tb_notification_type
                                       on notification.notification_type_id equals type.id into typeJoin
                                   from type in typeJoin.DefaultIfEmpty()
                                   where notification.receiver_user_id == userId
                                   orderby notification.created_date descending
                                   select new NotificationDto
                                   {
                                       Id = notification.id,
                                       Type = type != null ? type.short_text : null,
                                       ShortText = type != null ? type.short_text : null,
                                       LongText = type != null ? type.long_text : null,
                                       Href = notification.reff_id ?? (type != null ? type.href : null),
                                       IsSeen = notification.is_seen == true,
                                       CreatedDate = notification.created_date,
                                   })
            .Take(Math.Clamp(top, 1, 100))
            .ToListAsync();

        return notifications;
    }

    public async Task<BaseGeneralResponse> MarkSeenAsync(string userId, long id)
    {
        var notification = await _context.tb_notification
            .FirstOrDefaultAsync(e => e.id == id && e.receiver_user_id == userId);
        if (notification == null)
        {
            return Fail("Notification not found.");
        }

        notification.is_seen = true;
        await _context.SaveChangesAsync();
        return Success("Notification marked as read.");
    }

    public async Task<BaseGeneralResponse> MarkAllSeenAsync(string userId)
    {
        var notifications = await _context.tb_notification
            .Where(e => e.receiver_user_id == userId && e.is_seen != true)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.is_seen = true;
        }

        await _context.SaveChangesAsync();
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

        if (typeId == null || !await _context.tb_notification_type.AnyAsync(e => e.id == typeId, cancellationToken))
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
            await _context.tb_notification_type.AddAsync(type, cancellationToken);
            notification.notification_type_id = type.id;
        }
        else
        {
            // Type metadata is shared; per-notification links live in reff_id.
        }

        await _context.tb_notification.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

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
        catch
        {
            // Real-time delivery is best-effort; the notification stays in the database.
        }
    }

    private static BaseGeneralResponse Success(string message) => new()
    {
        Success = true,
        Code = BCIConst.SUCCESS_CODE,
        Message = message,
    };

    private static BaseGeneralResponse Fail(string message) => new()
    {
        Success = false,
        Code = BCIConst.ERROR_CODE,
        Message = message,
    };
}
