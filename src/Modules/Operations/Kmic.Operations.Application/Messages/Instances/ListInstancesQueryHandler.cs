using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Operations.Application.Messages.Instances;

/// <summary>
/// 处理 <see cref="ListInstancesQuery"/>
/// </summary>
public sealed class ListInstancesQueryHandler(IOperationsQueryDb db, TimeProvider time) : IQueryHandler<ListInstancesQuery, IReadOnlyList<InstanceItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyList<InstanceItem>>> Handle(ListInstancesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await db.MessagingInstances.OrderByDescending(i => i.HeartbeatAt).ThenBy(i => i.Id).ToListAsync(cancellationToken);
        var now = time.GetUtcNow();
        List<InstanceItem> items =
        [
            .. rows.Select(r => new InstanceItem(r.Id, r.Host, r.Roles, r.Build, r.StartedAt, r.HeartbeatAt, InstanceSubscriptions.IsAlive(r, now), InstanceSubscriptions.Read(r.Subscriptions))),
        ];
        return items;
    }
}
