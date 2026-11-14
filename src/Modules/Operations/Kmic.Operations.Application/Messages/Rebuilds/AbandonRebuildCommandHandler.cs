using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 处理 <see cref="AbandonRebuildCommand"/>
/// </summary>
public sealed class AbandonRebuildCommandHandler(IReadModelRebuildAdmin rebuilds) : ICommandHandler<AbandonRebuildCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(AbandonRebuildCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return RebuildOutcomes.Of(await rebuilds.AbandonAsync(command.Subscription, cancellationToken));
    }
}
