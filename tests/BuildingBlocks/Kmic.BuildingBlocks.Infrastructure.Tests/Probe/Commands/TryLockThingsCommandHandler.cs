using Kmic.BuildingBlocks.Application.Locking;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class TryLockThingsCommandHandler(ITransactionLocks locks, ProbeHooks hooks) : ICommandHandler<TryLockThingsCommand, LockedThings>
{
    public async ValueTask<Result<LockedThings>> Handle(TryLockThingsCommand command, CancellationToken cancellationToken)
    {
        var acquired = await locks.TryAcquireExclusiveAsync("Things", command.Ids, cancellationToken);

        // 获取锁后暂停执行，以便测试在另一连接上验证锁状态后再放行提交
        await hooks.BeforeReturnAsync(cancellationToken);
        return new LockedThings([.. acquired]);
    }
}
