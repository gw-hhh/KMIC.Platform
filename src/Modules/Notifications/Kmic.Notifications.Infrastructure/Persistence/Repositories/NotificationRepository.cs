using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRepository(NotificationsDbContext context)
    : EfRepository<NotificationsDbContext, Notification, NotificationId>(context), INotificationRepository
{
    protected override Error NotFound => NotificationsErrors.NotificationNotFound;

    public async Task<bool> ExistsForAsync(Guid sourceMessageId, CancellationToken ct) =>
        await Context.Set<Notification>().AnyAsync(n => n.SourceMessageId == sourceMessageId, ct);
}
