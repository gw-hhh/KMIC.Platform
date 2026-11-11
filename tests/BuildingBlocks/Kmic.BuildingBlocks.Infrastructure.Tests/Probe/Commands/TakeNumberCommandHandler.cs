using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class TakeNumberCommandHandler(INumberGenerator numbers) : ICommandHandler<TakeNumberCommand, TakenNumber>
{
    public async ValueTask<Result<TakenNumber>> Handle(TakeNumberCommand command, CancellationToken cancellationToken)
    {
        var number = await numbers.NextAsync(command.Rule, command.Partition, cancellationToken);
        return command.FailAfterTaking ? TicketErrors.Rejected : new TakenNumber(number);
    }
}
