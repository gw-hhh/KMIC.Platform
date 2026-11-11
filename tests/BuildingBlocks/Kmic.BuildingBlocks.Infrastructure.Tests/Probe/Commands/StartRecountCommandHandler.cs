using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class StartRecountCommandHandler(IBackgroundJobs jobs) : ICommandHandler<StartRecountCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(StartRecountCommand command, CancellationToken cancellationToken)
    {
        var enqueued = await jobs.EnqueueAsync(new RecountTicketJob(command.Id.Value, command.Batches), cancellationToken);
        return command.Fail && enqueued.IsSuccess ? TicketErrors.Rejected : enqueued;
    }
}
