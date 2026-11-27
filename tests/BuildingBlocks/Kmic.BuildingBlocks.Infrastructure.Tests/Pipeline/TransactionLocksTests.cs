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

public sealed class TransactionLocksTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private readonly TestIdentity _user = TestIdentity.User("取锁的人").InTree(_org).InOrgUnit(_org["总部"]);
    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    // 在独立连接上尝试非阻塞获取排他锁并立即释放，用于探测当前锁是否可用
    private async Task<bool> ExclusiveTryLockSucceedsAsync(Guid id)
    {
        await using var connection = await database.DataSource.OpenConnectionAsync(Ct);
        await using var transaction = await connection.BeginTransactionAsync(Ct);
        await using var command = new NpgsqlCommand("SELECT pg_try_advisory_xact_lock(@key)", connection, transaction);
        command.Parameters.AddWithValue("key", LockKeys.Of("Things", id));
        return (bool)(await command.ExecuteScalarAsync(Ct))!;
    }

    // 使命令在获取锁后暂停于处理器中，并返回已就绪信号与放行开关
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

        whileHeld.ShouldBeFalse("命令事务尚未提交且持有共享锁时，清理方不应获取到排他锁");
        afterCommit.ShouldBeTrue("命令事务提交后，咨询锁应已释放");
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

        // 若共享锁之间互斥阻塞，将无法同时抵达此处信号点
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
        held.ShouldAllBe(h => !h, "去重后的每个目标 ID 均应已被锁定，且重复 ID 不应导致死锁");
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

        taken.Ids.ShouldBe([idle], "应跳过已被业务命令持有共享锁的 ID，仅锁定空闲且去重后的 ID");
        idleWhileHeld.ShouldBeFalse("清理事务结束前，已获取的排他锁应持续持有");
        idleAfterCommit.ShouldBeTrue("清理事务提交后，排他锁应已释放");
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
        // 固定校验哈希值：确保业务端与清理端计算锁键的规则保持一致，若规则变更未同步则立即失败
        LockKeys.Of("Files", id).ShouldBe(-470_533_028_823_510_044L);
    }
}
