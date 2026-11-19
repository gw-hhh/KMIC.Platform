using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 放弃读模型重建
/// </summary>
public sealed class AbandonRebuildCommandHandler(IReadModelRebuildAdmin rebuilds) : ICommandHandler<AbandonRebuildCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(AbandonRebuildCommand command, CancellationToken cancellationToken)
    {
        return RebuildOutcomes.Of(await rebuilds.AbandonAsync(command.Subscription, cancellationToken));
    }
}
