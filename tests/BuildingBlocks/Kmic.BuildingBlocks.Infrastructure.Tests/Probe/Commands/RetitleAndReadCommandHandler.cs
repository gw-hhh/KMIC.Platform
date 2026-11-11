using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class RetitleAndReadCommandHandler(TicketRepository tickets, IQuerySender queries) : ICommandHandler<RetitleAndReadCommand, string>
{
    public async ValueTask<Result<string>> Handle(RetitleAndReadCommand command, CancellationToken cancellationToken)
    {
        var ticket = (await tickets.GetForUpdateAsync(command.Id, null, cancellationToken)).Value;
        if (command.ReadFirst)
        {
            var before = await queries.SendAsync(new GetTicketTitleQuery(command.Id), cancellationToken);
            ticket.Rename(command.Title);
            return before;
        }

        ticket.Rename(command.Title);
        return await queries.SendAsync(new GetTicketTitleQuery(command.Id), cancellationToken);
    }
}
