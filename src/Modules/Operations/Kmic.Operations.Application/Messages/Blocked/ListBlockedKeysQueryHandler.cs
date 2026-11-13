using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.Messages.Blocked;

/// <summary>
/// 处理 <see cref="ListBlockedKeysQuery"/>
/// </summary>
public sealed class ListBlockedKeysQueryHandler(IModuleMessageStores stores) : IQueryHandler<ListBlockedKeysQuery, IReadOnlyList<BlockedKeyItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyList<BlockedKeyItem>>> Handle(ListBlockedKeysQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        IEnumerable<IModuleMessageStore> selected = stores.All;
        if (query.Module is { Length: > 0 } module)
        {
            selected = stores.Find(module) is { } one ? [one] : [];
        }

        var lists = await Task.WhenAll(selected.Select(store => store.ListBlockedKeysAsync(cancellationToken)));
        var items = lists
            .SelectMany(keys => keys)
            .OrderByDescending(k => k.Waiting)
            .ThenBy(k => k.Module, StringComparer.Ordinal)
            .ThenBy(k => k.Subscription, StringComparer.Ordinal)
            .ThenBy(k => k.OrderingKey, StringComparer.Ordinal)
            .Select(k => new BlockedKeyItem(k.Module, k.Subscription, k.OrderingKey, k.DeadCount, k.Waiting, k.FirstDeadId, k.LastError))
            .ToList();
        return items;
    }
}
