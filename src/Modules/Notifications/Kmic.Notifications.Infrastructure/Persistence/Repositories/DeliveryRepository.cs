using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Notifications.Infrastructure.Persistence.Repositories;

internal sealed class DeliveryRepository(NotificationsDbContext context)
    : EfRepository<NotificationsDbContext, Delivery, DeliveryId>(context), IDeliveryRepository
{
    protected override Error NotFound => NotificationsErrors.DeliveryNotFound;

    public async Task<IReadOnlySet<string>> FindExistingAsync(string templateKey, string dedupKey, CancellationToken ct)
    {
        var rows = await Context.Set<Delivery>()
            .Where(d => d.TemplateKey == templateKey && d.DedupKey == dedupKey)
            .Select(d => new { d.RecipientKey, d.Channel })
            .ToListAsync(ct);
        return rows.Select(r => $"{r.RecipientKey}|{r.Channel.Code}").ToHashSet(StringComparer.Ordinal);
    }
}
