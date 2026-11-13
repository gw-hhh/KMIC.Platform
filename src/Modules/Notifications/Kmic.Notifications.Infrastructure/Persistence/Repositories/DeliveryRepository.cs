using Kmic.BuildingBlocks.Application.Querying;
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

    public async Task<bool> RetryAsync(DeliveryId id, DateTimeOffset? createdNotBefore, CancellationToken ct) =>
        await UpdateWhereAsync(
            d => d.Id == id
                && d.Status == DeliveryStatus.Failed
                && EF.Property<DateTimeOffset?>(d, DeliveryColumns.AbandonedAt) == null
                && (createdNotBefore == null || EF.Property<DateTimeOffset>(d, AuditColumns.CreatedAt) >= createdNotBefore),
            setters => setters
                .SetProperty(d => d.Status, DeliveryStatus.Pending)
                .SetProperty(d => EF.Property<int>(d, DeliveryColumns.Attempts), 0)
                .SetProperty(d => EF.Property<DateTimeOffset?>(d, DeliveryColumns.NextAttemptAt), (DateTimeOffset?)null)
                .SetProperty(d => EF.Property<string?>(d, DeliveryColumns.LastError), (string?)null),
            ct) == 1;

    public async Task<bool> AbandonAsync(DeliveryId id, string reason, DateTimeOffset now, CancellationToken ct) =>
        await UpdateWhereAsync(
            d => d.Id == id && d.Status == DeliveryStatus.Failed && EF.Property<DateTimeOffset?>(d, DeliveryColumns.AbandonedAt) == null,
            setters => setters
                .SetProperty(d => EF.Property<DateTimeOffset?>(d, DeliveryColumns.AbandonedAt), (DateTimeOffset?)now)
                .SetProperty(d => EF.Property<string?>(d, DeliveryColumns.AbandonReason), reason),
            ct) == 1;

    public async Task<int> PurgeAsync(DateTimeOffset createdBefore, CancellationToken ct) =>
        await Context.Set<Delivery>()
            .Where(d => EF.Property<DateTimeOffset>(d, AuditColumns.CreatedAt) < createdBefore
                && (d.Status == DeliveryStatus.Sent
                    || d.Status == DeliveryStatus.Skipped
                    || d.Status == DeliveryStatus.Captured
                    || (d.Status == DeliveryStatus.Failed && EF.Property<DateTimeOffset?>(d, DeliveryColumns.AbandonedAt) != null)))
            .ExecuteDeleteAsync(ct);

    public async Task<int> ClearSecretBodiesAsync(IReadOnlyCollection<string> templateKeys, CancellationToken ct) =>
        await UpdateWhereAsync(
            d => templateKeys.Contains(d.TemplateKey)
                && (d.Body != null || d.SmsParameters != null)
                && (d.Status == DeliveryStatus.Sent || d.Status == DeliveryStatus.Failed || d.Status == DeliveryStatus.Skipped),
            setters => setters
                .SetProperty(d => d.Body, (string?)null)
                .SetProperty(d => d.SmsParameters, (string?)null),
            ct);
}
