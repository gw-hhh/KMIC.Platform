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

        // 锁拿到之后停在这里，测试在另一个连接上检查锁，再放行让命令提交
        await hooks.BeforeReturnAsync(cancellationToken);
        return Result.Success();
    }
}
