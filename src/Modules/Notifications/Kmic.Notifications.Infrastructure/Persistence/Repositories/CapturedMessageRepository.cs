using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.Captured;
using Kmic.Notifications.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Infrastructure.Persistence.Repositories;

internal sealed class CapturedMessageRepository(NotificationsDbContext context)
    : EfRepository<NotificationsDbContext, CapturedMessage, CapturedMessageId>(context), ICapturedMessageRepository
{
    protected override Error NotFound => NotificationsErrors.CapturedUnavailable;

    public async Task<int> PurgeAsync(DateTimeOffset capturedBefore, CancellationToken ct) =>
        await Context.Set<CapturedMessage>().Where(c => c.CapturedAt < capturedBefore).ExecuteDeleteAsync(ct);
}
