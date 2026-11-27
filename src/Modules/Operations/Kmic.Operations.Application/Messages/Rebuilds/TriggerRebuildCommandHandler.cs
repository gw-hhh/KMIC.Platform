using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.Messages.Rebuilds;

// 直接委托框架重建管理接口，不纳入 Operations 本地事务
public sealed class TriggerRebuildCommandHandler(IReadModelRebuildAdmin rebuilds) : ICommandHandler<TriggerRebuildCommand>
{
    public async ValueTask<Result> Handle(TriggerRebuildCommand command, CancellationToken cancellationToken)
    {
        return RebuildOutcomes.Of(await rebuilds.TriggerAsync(command.Subscription, cancellationToken));
    }
}
