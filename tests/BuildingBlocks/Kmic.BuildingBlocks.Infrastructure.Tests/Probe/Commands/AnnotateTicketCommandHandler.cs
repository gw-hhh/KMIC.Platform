using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class AnnotateTicketCommandHandler(TicketRepository tickets) : ICommandHandler<AnnotateTicketCommand>
{
    public async ValueTask<Result> Handle(AnnotateTicketCommand command, CancellationToken cancellationToken)
    {
        var loaded = await tickets.GetForUpdateAsync(command.Id, null, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        loaded.Value.AddLine(command.Text);
        return Result.Success();
    }
}
