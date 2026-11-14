using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 启动读模型重建
/// </summary>
/// <remarks>
/// 框架在订阅方模块的事务中写入重建轮次和作业，不参与 Operations 命令事务
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
