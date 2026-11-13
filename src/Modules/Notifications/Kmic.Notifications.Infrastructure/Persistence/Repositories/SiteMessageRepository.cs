using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.Errors;
using Kmic.Notifications.Domain.SiteMessages;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Infrastructure.Persistence.Repositories;

internal sealed class SiteMessageRepository(NotificationsDbContext context)
    : EfRepository<NotificationsDbContext, SiteMessage, SiteMessageId>(context), ISiteMessageRepository
{
    protected override Error NotFound => NotificationsErrors.SiteMessageNotFound;

    public async Task<int> MarkAllReadAsync(UserId recipient, DateTimeOffset now, CancellationToken ct) =>
        await UpdateWhereAsync(
            m => m.RecipientUserId == recipient && m.ReadAt == null,
            setters => setters.SetProperty(m => m.ReadAt, (DateTimeOffset?)now),
            ct);

    public async Task<int> PurgeAsync(DateTimeOffset readBefore, DateTimeOffset unreadCreatedBefore, CancellationToken ct) =>
        await Context.Set<SiteMessage>()
            .Where(m => (m.ReadAt != null && m.ReadAt < readBefore) || (m.ReadAt == null && m.SentAt < unreadCreatedBefore))
            .ExecuteDeleteAsync(ct);
}
