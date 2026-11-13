using Kmic.BuildingBlocks.Application.Locking;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Pipeline;

/// <summary>
/// 事务级咨询锁：共享锁与排他锁互斥、共享锁之间不互相等、等锁超时是暂时冲突、锁随命令的事务释放
/// </summary>
public sealed class TransactionLocksTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private readonly TestIdentity _user = TestIdentity.User("取锁的人").InTree(_org).InOrgUnit(_org["总部"]);
    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    // 另一个连接上尝试取排他锁：取到了立刻释放，只回答“现在取不取得到”
    private async Task<bool> ExclusiveTryLockSucceedsAsync(Guid id)
    {
        await using var connection = await database.DataSource.OpenConnectionAsync(Ct);
        await using var transaction = await connection.BeginTransactionAsync(Ct);
        await using var command = new NpgsqlCommand("SELECT pg_try_advisory_xact_lock(@key)", connection, transaction);
        command.Parameters.AddWithValue("key", LockKeys.Of("Things", id));
        return (bool)(await command.ExecuteScalarAsync(Ct))!;
    }

    // 命令取到锁之后停在处理器里：先放起来，返回“已经停住”的信号和“放行”的开关
    private (Task Reached, TaskCompletionSource Release) HoldCommandsAfterTheyLock()
    {
        var reached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _harness.Hooks.OnBeforeReturn(async ct =>
        {
            reached.TrySetResult();
            await release.Task.WaitAsync(ct);
        });
        return (reached.Task, release);
    }

    [Fact]
    public async Task SharedLocks_KeepAnExclusiveTryLockOut_UntilTheCommandsTransactionEnds()
    {
        var id = Guid.CreateVersion7();
        var (reached, release) = HoldCommandsAfterTheyLock();
        await using var work = _harness.Begin(_user);
        var command = work.Sender.SendAsync(new LockThingsCommand([id], TimeSpan.FromSeconds(5)), Ct).AsTask();
        await reached.WaitAsync(TimeSpan.FromSeconds(30), Ct);

        var whileHeld = await ExclusiveTryLockSucceedsAsync(id);
        release.SetResult();
        (await command).ShouldBeSuccess();
        var afterCommit = await ExclusiveTryLockSucceedsAsync(id);

        whileHeld.ShouldBeFalse("共享锁还在命令的事务里，清理一方取不到排他锁");
        afterCommit.ShouldBeTrue("命令提交之后锁已经释放");
    }

    [Fact]
    public async Task TwoCommandsHoldingSharedLocksOnTheSameThing_DoNotWaitForEachOther()
    {
        var id = Guid.CreateVersion7();
        var arrived = 0;
        var bothArrived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _harness.Hooks.OnBeforeReturn(async ct =>
        {
            if (Interlocked.Increment(ref arrived) == 2)
            {
                bothArrived.SetResult();
            }

            await bothArrived.Task.WaitAsync(ct);
        });
        await using var first = _harness.Begin(_user);
        await using var second = _harness.Begin(_user);

        var firstTask = first.Sender.SendAsync(new LockThingsCommand([id], TimeSpan.FromSeconds(5)), Ct).AsTask();
        var secondTask = second.Sender.SendAsync(new LockThingsCommand([id], TimeSpan.FromSeconds(5)), Ct).AsTask();

        // 两个都取到了共享锁、停在处理器里；要是共享锁互相等，永远等不到“两个都到了”
        await bothArrived.Task.WaitAsync(TimeSpan.FromSeconds(30), Ct);
        (await firstTask).ShouldBeSuccess();
        (await secondTask).ShouldBeSuccess();
    }

    [Fact]
    public async Task WaitingBehindAnExclusiveLock_ForLongerThanTheTimeout_IsATransientConflict()
    {
        var id = Guid.CreateVersion7();
        await using var holder = await database.DataSource.OpenConnectionAsync(Ct);
        await using var transaction = await holder.BeginTransactionAsync(Ct);
        await using (var take = new NpgsqlCommand("SELECT pg_advisory_xact_lock(@key)", holder, transaction))
        {
            take.Parameters.AddWithValue("key", LockKeys.Of("Things", id));
            await take.ExecuteNonQueryAsync(Ct);
        }

        await using var work = _harness.Begin(_user);
        var started = TimeProvider.System.GetTimestamp();
        var result = await work.Sender.SendAsync(new LockThingsCommand([id], TimeSpan.FromMilliseconds(300)), Ct);
        var waited = TimeProvider.System.GetElapsedTime(started);

        result.ShouldBeFailure(CommonErrors.TransientConflict);
        waited.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(250));
        waited.ShouldBeLessThan(TimeSpan.FromSeconds(10));
        await transaction.RollbackAsync(Ct);
    }

    [Fact]
    public async Task ManyThings_AreAllLocked_InAFixedOrder_AndNoThingsIsFine()
    {
        var ids = Enumerable.Range(0, 5).Select(_ => Guid.CreateVersion7()).ToList();
        var (reached, release) = HoldCommandsAfterTheyLock();
        await using var work = _harness.Begin(_user);
        var command = work.Sender.SendAsync(new LockThingsCommand([.. ids, ids[0]], TimeSpan.FromSeconds(5)), Ct).AsTask();
        await reached.WaitAsync(TimeSpan.FromSeconds(30), Ct);

        var held = new List<bool>();
        foreach (var id in ids)
        {
            held.Add(await ExclusiveTryLockSucceedsAsync(id));
        }

        release.SetResult();
        (await command).ShouldBeSuccess();
        held.ShouldAllBe(h => !h, "每一个都被锁住了，重复的 ID 只锁一次");
        _harness.Hooks.OnBeforeReturn(null);
        await using var empty = _harness.Begin(_user);
        (await empty.Sender.SendAsync(new LockThingsCommand([], TimeSpan.FromSeconds(1)), Ct)).ShouldBeSuccess();
    }

    [Fact]
    public async Task TryAcquireExclusive_SkipsWhatABusinessCommandHoldsShared_AndTakesTheRest()
    {
        var busy = Guid.CreateVersion7();
        var idle = Guid.CreateVersion7();
        await using var business = await database.DataSource.OpenConnectionAsync(Ct);
        await using var businessTransaction = await business.BeginTransactionAsync(Ct);
        await using (var share = new NpgsqlCommand("SELECT pg_advisory_xact_lock_shared(@key)", business, businessTransaction))
        {
            share.Parameters.AddWithValue("key", LockKeys.Of("Things", busy));
            await share.ExecuteNonQueryAsync(Ct);
        }

        var (reached, release) = HoldCommandsAfterTheyLock();
        await using var work = _harness.Begin(_user);
        var cleanup = work.Sender.SendAsync(new TryLockThingsCommand([busy, idle, idle]), Ct).AsTask();
        await reached.WaitAsync(TimeSpan.FromSeconds(30), Ct);

        var idleWhileHeld = await ExclusiveTryLockSucceedsAsync(idle);
        release.SetResult();
        var taken = (await cleanup).ShouldBeSuccess();
        var idleAfterCommit = await ExclusiveTryLockSucceedsAsync(idle);
        await businessTransaction.RollbackAsync(Ct);

        taken.Ids.ShouldBe([idle], "业务命令持有共享锁的那个这次跳过，没人持有的取到，重复的 ID 只锁一次");
        idleWhileHeld.ShouldBeFalse("取到的排他锁在清理的事务里一直持有");
        idleAfterCommit.ShouldBeTrue("清理的事务提交之后锁释放");
    }

    [Fact]
    public async Task TryAcquireExclusive_OutsideACommand_Refuses()
    {
        await using var scope = _harness.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await scope.ServiceProvider.GetRequiredService<ITransactionLocks>().TryAcquireExclusiveAsync("Things", [Guid.CreateVersion7()], Ct));
    }

    [Fact]
    public async Task OutsideACommand_ItRefuses()
    {
        await using var scope = _harness.BeginAsSystem();
        var locks = _harness.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
        await using (locks)
        {
            await Should.ThrowAsync<InvalidOperationException>(async () =>
                await locks.ServiceProvider.GetRequiredService<ITransactionLocks>().AcquireSharedAsync("Things", [Guid.CreateVersion7()], TimeSpan.FromSeconds(1), Ct));
        }
    }

    [Fact]
    public void TheKey_IsStable_AndDependsOnTheScopeAndTheId()
    {
        var id = Guid.Parse("6f2a0f6e-0f64-4d8e-9a53-3a2b45c0b7aa");

        LockKeys.Of("Files", id).ShouldBe(LockKeys.Of("Files", id));
        LockKeys.Of("Files", id).ShouldNotBe(LockKeys.Of("Things", id));
        LockKeys.Of("Files", id).ShouldNotBe(LockKeys.Of("Files", Guid.Parse("6f2a0f6e-0f64-4d8e-9a53-3a2b45c0b7ab")));
        // 写死一个值：规则变了，清理和业务两边没同步改，互斥就悄悄失效，这条测试要报出来
        LockKeys.Of("Files", id).ShouldBe(-470_533_028_823_510_044L);
    }
}
