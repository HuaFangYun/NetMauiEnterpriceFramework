using Microsoft.EntityFrameworkCore;

using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Data;

public static class NotificationSeeder
{
    public static async Task SeedAsync(__PROJECT__DbContext context, CancellationToken cancellationToken = default)
    {
        if (!await context.tb_notification_type.AnyAsync(e => e.id == NotificationTypeIds.Approval, cancellationToken))
        {
            await context.tb_notification_type.AddAsync(new tb_notification_type
            {
                id = NotificationTypeIds.Approval,
                short_text = "Approval",
                long_text = "There is an approval update.",
                href = null,
                is_send_mail = false,
                is_send_web = true,
                created_by = "system",
                created_date = DateTime.Now,
            }, cancellationToken);
        }

        if (!await context.tb_notification_type.AnyAsync(e => e.id == NotificationTypeIds.Export, cancellationToken))
        {
            await context.tb_notification_type.AddAsync(new tb_notification_type
            {
                id = NotificationTypeIds.Export,
                short_text = "Export ready",
                long_text = "Your export is ready to download.",
                href = null,
                is_send_mail = false,
                is_send_web = true,
                created_by = "system",
                created_date = DateTime.Now,
            }, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
