using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>
/// 处理 <see cref="ExportAuditCommand"/>
/// </summary>
public sealed class ExportAuditCommandHandler(IBackgroundJobs jobs) : ICommandHandler<ExportAuditCommand, JobAccepted>
{
    /// <inheritdoc />
    public async ValueTask<Result<JobAccepted>> Handle(ExportAuditCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var queued = await jobs.EnqueueAsync(
            new ExportAuditJob(command.From, command.To, command.ActorUserId, command.ActualUserId, command.Module, command.Action, command.ResourceType, command.ResourceId),
            cancellationToken);
        return queued.IsSuccess ? new JobAccepted(queued.Value) : queued.Error;
    }
}
