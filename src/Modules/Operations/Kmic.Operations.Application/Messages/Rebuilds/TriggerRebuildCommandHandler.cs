using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 处理 <see cref="TriggerRebuildCommand"/>
/// </summary>
/// <remarks>
/// 写的是订阅方模块的轮次和作业表，由框架在自己的事务里完成，不在 Operations 命令的事务里
/// </remarks>
public sealed class TriggerRebuildCommandHandler(IReadModelRebuildAdmin rebuilds) : ICommandHandler<TriggerRebuildCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(TriggerRebuildCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return RebuildOutcomes.Of(await rebuilds.TriggerAsync(command.Subscription, cancellationToken));
    }
}
