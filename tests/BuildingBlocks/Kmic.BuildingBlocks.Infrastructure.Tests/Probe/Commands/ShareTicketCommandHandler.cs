using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class ShareTicketCommandHandler(TicketRepository tickets, IIntegrationEventPublisher publisher) : ICommandHandler<ShareTicketCommand>
{
    public async ValueTask<Result> Handle(ShareTicketCommand command, CancellationToken cancellationToken)
    {
        var loaded = await tickets.GetForUpdateAsync(command.Id, null, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var ticket = loaded.Value;
        foreach (var title in command.Titles)
        {
            ticket.Rename(title);
            publisher.Add(new TicketChanged(new TicketSnapshot(ticket.Id.Value, ticket.Title, ticket.Status.Code)));
        }

        return command.Fail ? TicketErrors.Rejected : Result.Success();
    }
}
