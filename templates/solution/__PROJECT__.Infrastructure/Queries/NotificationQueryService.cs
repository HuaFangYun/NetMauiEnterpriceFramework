using Microsoft.EntityFrameworkCore;

using __PROJECT__.Application.DTOs.Notification;
using __PROJECT__.Application.Interfaces.Queries;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Queries;

public class NotificationQueryService : INotificationQueryService
{
    private readonly __PROJECT__DbContext _context;

    public NotificationQueryService(__PROJECT__DbContext context) => _context = context;

    public async Task<List<NotificationDto>> GetRecentAsync(
        string userId,
        int top = 20,
        CancellationToken cancellationToken = default)
    {
        return await (from notification in _context.tb_notification
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
            .ToListAsync(cancellationToken);
    }
}
