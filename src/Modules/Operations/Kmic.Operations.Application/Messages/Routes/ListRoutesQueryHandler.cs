using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Application.Messages.Instances;
using Kmic.Operations.Domain.Messages;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 处理 <see cref="ListRoutesQuery"/>
/// </summary>
public sealed class ListRoutesQueryHandler(IOperationsQueryDb db, TimeProvider time) : IQueryHandler<ListRoutesQuery, IReadOnlyList<RouteItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyList<RouteItem>>> Handle(ListRoutesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var statuses = EnumerationFilter.Parse<MessageRouteStatus>(query.Status, "status");
        if (statuses.IsFailure)
        {
            return statuses.Error;
        }

        var wanted = statuses.Value;
        var routes = await db.MessageRoutes.OrderBy(r => r.Module).ThenBy(r => r.Subscription).ThenBy(r => r.MessageType).ToListAsync(cancellationToken);
        if (wanted.Count > 0)
        {
            routes = [.. routes.Where(r => wanted.Contains(MessageRouteStatus.FromCode(r.Status)))];
        }

        // 只看现在还在运行的实例：已经停掉的构建不影响能不能终结
        var now = time.GetUtcNow();
        var alive = (await db.MessagingInstances.ToListAsync(cancellationToken)).Where(i => InstanceSubscriptions.IsAlive(i, now)).ToList();
        var declared = alive
            .SelectMany(i => InstanceSubscriptions.Read(i.Subscriptions).Select(s => (Instance: i, Subscription: s)))
            .ToLookup(x => (x.Subscription.MessageType, x.Subscription.Subscription));

        List<RouteItem> items =
        [
            .. routes.Select(r => new RouteItem(
                r.MessageType,
                r.Subscription,
                r.Module,
                MessageRouteStatus.FromCode(r.Status),
                r.RegisteredBuild,
                r.RegisteredAt,
                r.NeedsInitialization,
                r.DisplayName,
                r.RetiredReason,
                r.FinalizedAt,
                r.FinalizedBy,
                [.. declared[(r.MessageType, r.Subscription)]
                    .GroupBy(x => x.Instance.Build, StringComparer.Ordinal)
                    .OrderBy(g => g.Key, StringComparer.Ordinal)
                    .Select(g => new RouteBuild(g.Key, g.Count()))])),
        ];
        return items;
    }
}
