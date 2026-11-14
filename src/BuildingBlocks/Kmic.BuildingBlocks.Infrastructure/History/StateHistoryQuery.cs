using System.Collections.Frozen;
using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.History;

/// <summary>
/// <see cref="IStateHistoryQuery"/> 的实现：从聚合所在模块的历史表读，人员和角色经 <see cref="IActorReferences"/> 取显示名
/// </summary>
internal sealed class StateHistoryQuery(ModuleQueryContexts contexts, IActorReferences actors) : IStateHistoryQuery
{
    private static readonly FrozenDictionary<Guid, ReferenceInfo> _none = FrozenDictionary<Guid, ReferenceInfo>.Empty;

    /// <inheritdoc />
    public async Task<IReadOnlyList<StateHistoryItem>> GetAsync<TAggregate>(Guid aggregateId, CancellationToken ct)
        where TAggregate : IAggregateRoot
    {
        var db = contexts.Get(ModuleNames.Of(typeof(TAggregate)));
        var rows = await db.Set<StateHistoryRecord>(StateHistory.TableName(typeof(TAggregate)))
            .Where(r => r.AggregateId == aggregateId)
            .OrderBy(r => r.OccurredAt)
            .ThenBy(r => r.Id)
            .ToListAsync(ct);
        if (rows.Count == 0)
        {
            return [];
        }

        var userIds = rows
            .SelectMany(r => new[] { r.ActorUserId, r.ActualUserId, r.OnBehalfOfUserId })
            .OfType<Guid>()
            .Distinct()
            .ToList();
        var roleIds = rows.Select(r => r.RoleId).OfType<Guid>().Distinct().ToList();
        var users = userIds.Count == 0 ? _none : await actors.ResolveUsersAsync(userIds, ct);
        var roles = roleIds.Count == 0 ? _none : await actors.ResolveRolesAsync(roleIds, ct);

        return
        [
            .. rows.Select(r => new StateHistoryItem(
                r.StateName,
                r.FromCode,
                r.ToCode,
                r.ActionCode,
                r.Reason,
                r.Revision,
                Find(users, r.ActorUserId),
                Find(users, r.ActualUserId),
                Find(roles, r.RoleId),
                Find(users, r.OnBehalfOfUserId),
                r.OccurredAt)),
        ];
    }

    // 查不到的按已删除显示，保留 ID
    private static ReferenceInfo? Find(IReadOnlyDictionary<Guid, ReferenceInfo> resolved, Guid? id) =>
        id is { } value
            ? resolved.GetValueOrDefault(value) ?? new ReferenceInfo(value, "已删除", null, ReferenceStatus.Deleted)
            : null;
}
