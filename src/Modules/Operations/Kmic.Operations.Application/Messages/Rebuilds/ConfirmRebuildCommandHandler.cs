using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.Messages.Rebuilds;

public sealed class ConfirmRebuildCommandHandler(IReadModelRebuildAdmin rebuilds) : ICommandHandler<ConfirmRebuildCommand>
{
    public async ValueTask<Result> Handle(ConfirmRebuildCommand command, CancellationToken cancellationToken)
    {
        return RebuildOutcomes.Of(await rebuilds.ConfirmAsync(command.Subscription, cancellationToken));
    }
}
