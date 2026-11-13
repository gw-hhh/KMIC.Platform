using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.Deliveries;
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

    public async Task<int> PurgeAsync(DateTimeOffset createdBefore, CancellationToken ct) =>
        await Context.Set<Notification>()
            .Where(n => EF.Property<DateTimeOffset>(n, AuditColumns.CreatedAt) < createdBefore && !Context.Set<Delivery>().Any(d => d.NotificationId == n.Id))
            .ExecuteDeleteAsync(ct);
}
