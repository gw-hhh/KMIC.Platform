using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 处理 <see cref="ConfirmRebuildCommand"/>
/// </summary>
public sealed class ConfirmRebuildCommandHandler(IReadModelRebuildAdmin rebuilds) : ICommandHandler<ConfirmRebuildCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(ConfirmRebuildCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return RebuildOutcomes.Of(await rebuilds.ConfirmAsync(command.Subscription, cancellationToken));
    }
}
