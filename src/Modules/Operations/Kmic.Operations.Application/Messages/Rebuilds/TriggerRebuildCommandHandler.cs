using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 启动读模型重建
/// </summary>
/// <remarks>
/// 不在 Operations 的命令事务里
/// </remarks>
public sealed class TriggerRebuildCommandHandler(IReadModelRebuildAdmin rebuilds) : ICommandHandler<TriggerRebuildCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(TriggerRebuildCommand command, CancellationToken cancellationToken)
    {
        return RebuildOutcomes.Of(await rebuilds.TriggerAsync(command.Subscription, cancellationToken));
    }
}
