using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class ShareTicketExemptCommandHandler(TicketRepository tickets, IIntegrationEventPublisher publisher) : ICommandHandler<ShareTicketExemptCommand>
{
    public async ValueTask<Result> Handle(ShareTicketExemptCommand command, CancellationToken cancellationToken)
    {
        var loaded = await tickets.GetForUpdateAsync(command.Id, null, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var ticket = loaded.Value;
        ticket.Rename(command.Title);
        publisher.Add(new TicketChanged(new TicketSnapshot(ticket.Id.Value, ticket.Title, ticket.Status.Code)));
        return Result.Success();
    }
}
