using System.Globalization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class StampTicketsCommandHandler(TicketRepository tickets, ProbeHooks hooks) : ICommandHandler<StampTicketsCommand, ScheduledJobOutcome>
{
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(StampTicketsCommand command, CancellationToken cancellationToken)
    {
        hooks.ScheduledFires.Enqueue(command.ScheduledFireTime);
        await hooks.BeforeReturnAsync(cancellationToken);
        if (hooks.StampTicket is { } id)
        {
            var loaded = await tickets.GetForUpdateAsync(id, null, cancellationToken);
            if (loaded.IsFailure)
            {
                return loaded.Error;
            }

            loaded.Value.AddLine("定时 " + command.ScheduledFireTime.ToString("HH:mm", CultureInfo.InvariantCulture));
        }

        // 失败时上面加的那一行随命令一起回滚
        return hooks.FailScheduled ? TicketErrors.Rejected : new ScheduledJobOutcome(1, "已盖章");
    }
}
