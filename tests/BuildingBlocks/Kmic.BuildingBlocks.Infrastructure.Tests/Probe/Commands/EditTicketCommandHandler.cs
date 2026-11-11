using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class EditTicketCommandHandler(TicketRepository tickets, IAuditContext audit) : ICommandHandler<EditTicketCommand>
{
    public async ValueTask<Result> Handle(EditTicketCommand command, CancellationToken cancellationToken)
    {
        var loaded = command.Restore
            ? await tickets.RestoreAsync(command.Id, null, cancellationToken)
            : await tickets.GetForUpdateAsync(command.Id, null, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        command.Edit?.Invoke(loaded.Value);
        if (command.Remove)
        {
            tickets.Remove(loaded.Value);
        }

        if (command.Detail is { } detail)
        {
            audit.Add("detail", detail);
            audit.AddTarget("Ticket", command.Id.Value);
        }

        return Result.Success();
    }
}
