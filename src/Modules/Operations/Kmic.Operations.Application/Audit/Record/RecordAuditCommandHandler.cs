using System.Net;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Audit;

namespace Kmic.Operations.Application.Audit.Record;

/// <summary>
/// 处理 <see cref="RecordAuditCommand"/>
/// </summary>
public sealed class RecordAuditCommandHandler(IAuditRepository audits) : ICommandHandler<RecordAuditCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(RecordAuditCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var audit = command.Audit;
        var id = AuditLogId.From(audit.AuditId);
        if (await audits.ExistsAsync(id, cancellationToken))
        {
            return Result.Success();
        }

        audits.Add(AuditLog.Record(
            id,
            audit.OccurredAt,
            audit.Module,
            audit.Action,
            audit.Origin,
            audit.TraceId,
            JsonSerializer.Serialize(audit.Targets, KmicJsonOptions.Stored),
            JsonSerializer.Serialize(audit.Changes, KmicJsonOptions.Stored),
            JsonSerializer.Serialize(audit.Details, KmicJsonOptions.Stored),
            audit.ActorUserId is { } actor ? UserId.From(actor) : null,
            audit.ActualUserId is { } actual ? UserId.From(actual) : null,
            audit.RoleId is { } role ? RoleId.From(role) : null,
            audit.SessionId is { } session ? SessionId.From(session) : null,
            audit.OnBehalfOfUserId is { } onBehalf ? UserId.From(onBehalf) : null,
            IPAddress.TryParse(audit.ClientIp, out var ip) ? ip : null));
        return Result.Success();
    }
}
