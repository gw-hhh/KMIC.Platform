using Kmic.BuildingBlocks.Application.Locking;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class LockThingsCommandHandler(ITransactionLocks locks, ProbeHooks hooks) : ICommandHandler<LockThingsCommand>
{
    public async ValueTask<Result> Handle(LockThingsCommand command, CancellationToken cancellationToken)
    {
        var locked = await locks.AcquireSharedAsync("Things", command.Ids, command.Timeout, cancellationToken);
        if (locked.IsFailure)
        {
            return locked;
        }

        // 获取锁后暂停执行，以便测试在另一连接上验证锁状态后再放行提交
        await hooks.BeforeReturnAsync(cancellationToken);
        return Result.Success();
    }
}
