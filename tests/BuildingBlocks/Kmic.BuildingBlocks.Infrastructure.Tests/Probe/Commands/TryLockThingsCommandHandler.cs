using Kmic.BuildingBlocks.Application.Locking;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class TryLockThingsCommandHandler(ITransactionLocks locks, ProbeHooks hooks) : ICommandHandler<TryLockThingsCommand, LockedThings>
{
    public async ValueTask<Result<LockedThings>> Handle(TryLockThingsCommand command, CancellationToken cancellationToken)
    {
        var acquired = await locks.TryAcquireExclusiveAsync("Things", command.Ids, cancellationToken);

        // 锁拿到之后停在这里，测试在另一个连接上检查锁，再放行让命令提交
        await hooks.BeforeReturnAsync(cancellationToken);
        return new LockedThings([.. acquired]);
    }
}
