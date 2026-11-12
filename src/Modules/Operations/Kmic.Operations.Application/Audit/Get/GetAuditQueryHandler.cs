using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;
using Kmic.Operations.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Operations.Application.Audit.Get;

/// <summary>
/// 处理 <see cref="GetAuditQuery"/>
/// </summary>
public sealed class GetAuditQueryHandler(IOperationsQueryDb db, IUserReferences users, IRoleReferences roles) : IQueryHandler<GetAuditQuery, AuditDetail>
{
    /// <inheritdoc />
    public async ValueTask<Result<AuditDetail>> Handle(GetAuditQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var entry = await db.AuditLogs
            .Where(a => a.Id == query.Id)
            .Select(a => new
            {
                a.OccurredAt,
                ReceivedAt = EF.Property<DateTimeOffset>(a, AuditColumns.CreatedAt),
                a.Module,
                a.Action,
                a.Origin,
                a.ActorUserId,
                a.ActualUserId,
                a.RoleId,
                a.SessionId,
                a.OnBehalfOfUserId,
                a.ClientIp,
                a.TraceId,
                a.Targets,
                a.Changes,
                a.Details,
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (entry is null)
        {
            return OperationsErrors.AuditNotFound;
        }

        var userNames = await users.ResolveAsync([.. new[] { entry.ActorUserId?.Value, entry.ActualUserId?.Value }.OfType<Guid>().Distinct()], cancellationToken);
        var roleNames = entry.RoleId is { } role
            ? await roles.ResolveAsync([role.Value], cancellationToken)
            : new Dictionary<Guid, ReferenceInfo>();
        return new AuditDetail(
            query.Id.Value,
            entry.OccurredAt,
            entry.ReceivedAt,
            entry.Module,
            entry.Action,
            entry.Origin,
            entry.ActorUserId?.Value,
            entry.ActorUserId is { } actor ? userNames.GetValueOrDefault(actor.Value)?.DisplayName : null,
            entry.ActualUserId?.Value,
            entry.ActualUserId is { } actual ? userNames.GetValueOrDefault(actual.Value)?.DisplayName : null,
            entry.RoleId?.Value,
            entry.RoleId is { } roleId ? roleNames.GetValueOrDefault(roleId.Value)?.DisplayName : null,
            entry.SessionId?.Value,
            entry.OnBehalfOfUserId?.Value,
            entry.ClientIp?.ToString(),
            entry.TraceId,
            JsonSerializer.Deserialize<List<AuditTargetItem>>(entry.Targets, KmicJsonOptions.Stored) ?? [],
            Element(entry.Changes),
            Element(entry.Details));
    }

    private static JsonElement Element(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
