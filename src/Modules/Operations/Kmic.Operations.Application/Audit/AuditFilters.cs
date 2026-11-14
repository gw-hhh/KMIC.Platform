using System.Text.Json;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Operations.Domain.Audit;

namespace Kmic.Operations.Application.Audit;

/// <summary>
/// 审计记录的筛选条件：列表和导出用同一份，导出的就是列表筛出来的全部
/// </summary>
internal static class AuditFilters
{
    /// <summary>
    /// 套上筛选条件，没给的条件不筛
    /// </summary>
    public static IQueryable<AuditLog> Apply(
        IOperationsQueryDb db,
        DateTimeOffset from,
        DateTimeOffset to,
        UserId? actorUserId,
        UserId? actualUserId,
        string? module,
        string? action,
        string? resourceType,
        Guid? resourceId)
    {
        var audits = db.AuditLogs.Where(a => a.OccurredAt >= from && a.OccurredAt < to);
        if (actorUserId is { } actor)
        {
            audits = audits.Where(a => a.ActorUserId == actor);
        }

        if (actualUserId is { } actual)
        {
            audits = audits.Where(a => a.ActualUserId == actual);
        }

        if (module is { Length: > 0 })
        {
            audits = audits.Where(a => a.Module == module);
        }

        if (action is { Length: > 0 })
        {
            audits = audits.Where(a => a.Action == action);
        }

        return TargetFilter(resourceType, resourceId) is { } targets ? db.WithTarget(audits, targets) : audits;
    }

    // 只写给了的条件：对象类型和对象 ID 可以只给一个
    private static string? TargetFilter(string? resourceType, Guid? resourceId)
    {
        if (resourceType is not { Length: > 0 } && resourceId is null)
        {
            return null;
        }

        var target = new Dictionary<string, object>(StringComparer.Ordinal);
        if (resourceType is { Length: > 0 } type)
        {
            target["resourceType"] = type;
        }

        if (resourceId is { } id)
        {
            target["resourceId"] = id;
        }

        return JsonSerializer.Serialize(new[] { target }, KmicJsonOptions.Stored);
    }
}
