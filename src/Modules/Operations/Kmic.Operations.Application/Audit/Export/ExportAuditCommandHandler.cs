using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.Audit.Export;

public sealed class ExportAuditCommandHandler(IBackgroundJobs jobs) : ICommandHandler<ExportAuditCommand, JobAccepted>
{
    public async ValueTask<Result<JobAccepted>> Handle(ExportAuditCommand command, CancellationToken cancellationToken)
    {
        return await jobs.EnqueueAsync(
            new ExportAuditJob(command.From, command.To, command.ActorUserId, command.ActualUserId, command.Module, command.Action, command.ResourceType, command.ResourceId),
            cancellationToken);
    }
}
