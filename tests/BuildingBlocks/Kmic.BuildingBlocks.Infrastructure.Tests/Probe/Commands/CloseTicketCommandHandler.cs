using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class CloseTicketCommandHandler(TicketRepository tickets, IAfterCommit afterCommit, ProbeHooks hooks) : ICommandHandler<CloseTicketCommand>
{
    public async ValueTask<Result> Handle(CloseTicketCommand command, CancellationToken cancellationToken)
    {
        var loaded = await tickets.GetForUpdateAsync(command.Id, null, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        loaded.Value.Close();
        afterCommit.Enqueue(_ =>
        {
            hooks.AfterCommitRuns++;
            return ValueTask.CompletedTask;
        });
        return Result.Success();
    }
}
