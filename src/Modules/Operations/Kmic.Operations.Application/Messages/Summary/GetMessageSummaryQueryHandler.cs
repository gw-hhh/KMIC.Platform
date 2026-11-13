using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.Messages.Summary;

/// <summary>
/// 处理 <see cref="GetMessageSummaryQuery"/>
/// </summary>
public sealed class GetMessageSummaryQueryHandler(IModuleMessageStores stores) : IQueryHandler<GetMessageSummaryQuery, MessageSummaryResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<MessageSummaryResponse>> Handle(GetMessageSummaryQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var retiring = await stores.GetRetiringSubscriptionsAsync(cancellationToken);
        var backlogs = await Task.WhenAll(stores.All.Select(store => store.GetBacklogAsync(cancellationToken)));
        var modules = backlogs
            .Select(b => new ModuleBacklogItem(
                b.Module,
                new OutboxBacklogItem(b.Outbox.Pending, Seconds(b.Outbox.OldestAge), b.Outbox.Dead),
                [.. b.Subscriptions.Select(s => new SubscriptionBacklogItem(s.Subscription, s.Pending, Seconds(s.OldestAge), s.Dead, s.Abandoned, s.BlockedKeys, retiring.Contains(s.Subscription)))]))
            .ToList();
        return new MessageSummaryResponse(modules);
    }

    private static long Seconds(TimeSpan age) => (long)Math.Floor(age.TotalSeconds);
}
