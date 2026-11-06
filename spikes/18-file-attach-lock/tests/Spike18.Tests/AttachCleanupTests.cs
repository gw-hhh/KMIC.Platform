using Npgsql;
using Shouldly;
using Xunit;
using static Spike18.Tests.Files;

namespace Spike18.Tests;

// 原型 #18：05-11 第 3.3 节补充说明的关联与清理互斥协议
public sealed class AttachCleanupTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(5);

    private readonly NpgsqlDataSource _db = fixture.Source;

    [Fact]
    public async Task Commit_waiting_for_sync_replication_keeps_the_lock_past_expiry()
    {
        var file = await UploadAsync(_db, TimeSpan.FromSeconds(1));
        await fixture.RequireMissingStandbyAsync();
        try
        {
            await using var attach = await AttachTransaction.BeginAsync(_db, [file], waitForStandby: true, _lockTimeout);
            await attach.LockAsync();
            (await attach.CheckAttachableAsync(_db)).ShouldBeTrue();
            var messageId = await attach.WriteAsync(Guid.CreateVersion7());

            // 提交停在等待同步复制，期间文件跨过到期时刻（到期由数据库时钟判断）
            var commit = attach.CommitAsync();
            await WaitForAsync(_db, attach.ProcessId, "SyncRep");
            await Db.ExecAsync(_db, "SELECT pg_sleep(1.5)");

            // 提交尚未对其他会话可见，共享锁仍被持有：清理取不到排他锁，跳过
            (await AttachmentsAsync(_db, file)).ShouldBe(0);
            (await CleanupTransaction.PurgeExpiredAsync(_db)).ShouldNotContain(file);
            (await StatusAsync(_db, file)).ShouldBe("Available");

            await fixture.ReleaseStandbyWaitAsync();
            await commit;

            // 提交可见之后，清理能取到锁，但探查到未完成的 AttachFiles，仍然跳过
            (await CleanupTransaction.PurgeExpiredAsync(_db)).ShouldNotContain(file);
            await DeliverAndProcessAsync(_db, messageId);

            (await StatusAsync(_db, file)).ShouldBe("Attached");
            (await CleanupTransaction.PurgeExpiredAsync(_db)).ShouldNotContain(file);
        }
        finally
        {
            await fixture.ReleaseStandbyWaitAsync();
        }
    }

    [Fact]
    public async Task Lock_is_released_only_after_the_commit_is_visible()
    {
        var file = await UploadAsync(_db, TimeSpan.Zero);
        await fixture.RequireMissingStandbyAsync();
        try
        {
            await using var attach = await AttachTransaction.BeginAsync(_db, [file], waitForStandby: true, _lockTimeout);
            await attach.LockAsync();
            await attach.WriteAsync(Guid.CreateVersion7());
            var commit = attach.CommitAsync();
            await WaitForAsync(_db, attach.ProcessId, "SyncRep");

            // 释放等待的同时反复尝试取排他锁；第一次取到锁时，提交必须已经可见
            var release = fixture.ReleaseStandbyWaitAsync();
            await using var probe = await _db.OpenConnectionAsync(TestContext.Current.CancellationToken);
            for (var attempt = 0; ; attempt++)
            {
                await using var transaction = await probe.BeginTransactionAsync(TestContext.Current.CancellationToken);
                var locked = (bool)(await Db.QueryAsync(probe, transaction, "SELECT pg_try_advisory_xact_lock(@key)", ("key", FileLocks.Key(file))))[0][0]!;
                if (locked)
                {
                    var visible = (long)(await Db.QueryAsync(probe, transaction, """
                        SELECT count(*) FROM quotation."OutboxMessage" WHERE "SubjectIds" && ARRAY[@file]::uuid[]
                        """, ("file", file)))[0][0]!;
                    visible.ShouldBe(1, $"第 {attempt} 次尝试取到锁时提交还不可见");
                    break;
                }

                await transaction.RollbackAsync(TestContext.Current.CancellationToken);
            }

            await release;
            await commit;
        }
        finally
        {
            await fixture.ReleaseStandbyWaitAsync();
        }
    }

    [Fact]
    public async Task Time_based_cleanup_deletes_a_file_whose_attach_is_committing()
    {
        var file = await UploadAsync(_db, TimeSpan.FromSeconds(1));
        await fixture.RequireMissingStandbyAsync();
        try
        {
            await using var attach = await AttachTransaction.BeginAsync(_db, [file], waitForStandby: true, _lockTimeout);
            await attach.LockAsync();
            (await attach.CheckAttachableAsync(_db)).ShouldBeTrue();
            var messageId = await attach.WriteAsync(Guid.CreateVersion7());
            var commit = attach.CommitAsync();
            await WaitForAsync(_db, attach.ProcessId, "SyncRep");
            await Db.ExecAsync(_db, "SELECT pg_sleep(1.5)");

            // 对照组：只按时间判断的清理在提交等待期间把文件改为 Deleting
            (await CleanupTransaction.PurgeExpiredByTimeAsync(_db)).ShouldBeGreaterThanOrEqualTo(1);

            await fixture.ReleaseStandbyWaitAsync();
            await commit;
            await DeliverToInboxAsync(_db, messageId);

            // 业务已提交的关联指向一个正在删除的文件
            (await AttachmentsAsync(_db, file)).ShouldBe(1);
            (await ProcessAttachFilesAsync(_db, messageId)).ShouldBe(0);
            (await StatusAsync(_db, file)).ShouldBe("Deleting");
        }
        finally
        {
            await fixture.ReleaseStandbyWaitAsync();
        }
    }

    [Fact]
    public async Task Cleanup_first_makes_attach_wait_and_then_reject()
    {
        var file = await UploadAsync(_db, TimeSpan.Zero);
        await using var cleanup = await CleanupTransaction.BeginAsync(_db);
        (await cleanup.TryLockAsync(file)).ShouldBeTrue();
        (await cleanup.MarkDeletingAsync(file)).ShouldBeTrue();

        await using var attach = await AttachTransaction.BeginAsync(_db, [file], waitForStandby: false, _lockTimeout);
        var locking = attach.LockAsync();
        await WaitForAsync(_db, attach.ProcessId, "advisory");
        await cleanup.CommitAsync();
        await locking;

        // 取到共享锁时读到的已是 Deleting，关联被拒绝
        (await attach.CheckAttachableAsync(_db)).ShouldBeFalse();
        await attach.RollbackAsync();
    }

    [Fact]
    public async Task Attach_gives_up_after_the_lock_timeout()
    {
        var file = await UploadAsync(_db, TimeSpan.Zero);
        await using var cleanup = await CleanupTransaction.BeginAsync(_db);
        (await cleanup.TryLockAsync(file)).ShouldBeTrue();

        await using var attach = await AttachTransaction.BeginAsync(_db, [file], waitForStandby: false, TimeSpan.FromSeconds(1));
        var error = await Should.ThrowAsync<PostgresException>(attach.LockAsync);

        // 映射为 503 Common.TransientConflict（retryable）
        error.SqlState.ShouldBe(PostgresErrorCodes.LockNotAvailable);
        await cleanup.CommitAsync();
    }

    // 清理的三步与 AttachFiles 投递的三步的全部 20 种交错
    public static TheoryData<string> Interleavings()
    {
        var data = new TheoryData<string>();
        for (var mask = 0; mask < 64; mask++)
        {
            if (int.PopCount(mask) == 3)
            {
                data.Add(string.Concat(Enumerable.Range(0, 6).Select(i => (mask & (1 << i)) != 0 ? 'C' : 'D')));
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Interleavings))]
    public async Task Probing_outbox_before_inbox_never_misses_the_message(string order)
    {
        (await RunInterleavingAsync(order, outboxFirst: true)).ShouldBeFalse();
    }

    [Fact]
    public async Task Probing_inbox_before_outbox_can_miss_the_message()
    {
        var misses = 0;
        foreach (var order in Interleavings().Select(row => row.Data))
        {
            misses += await RunInterleavingAsync(order, outboxFirst: false) ? 1 : 0;
        }

        // 对照组：先查 Inbox、后查 Outbox，消息在两次查询之间从 Outbox 移到 Inbox 时两边都查不到
        misses.ShouldBeGreaterThan(0);
        TestContext.Current.TestOutputHelper?.WriteLine($"反序探查漏判 {misses} / 20 种交错");
    }

    // 返回清理是否把一个已关联（或正在关联）的文件改为了 Deleting
    private async Task<bool> RunInterleavingAsync(string order, bool outboxFirst)
    {
        var file = await UploadAsync(_db, TimeSpan.Zero);
        await using (var attach = await AttachTransaction.BeginAsync(_db, [file], waitForStandby: false, _lockTimeout))
        {
            await attach.LockAsync();
            await attach.WriteAsync(Guid.CreateVersion7());
            await attach.CommitAsync();
        }

        var messageId = (Guid)(await Db.QueryAsync(_db, """
            SELECT "Id" FROM quotation."OutboxMessage" WHERE "SubjectIds" && ARRAY[@file]::uuid[]
            """, ("file", file)))[0][0]!;
        Func<Task>[] delivery =
        [
            () => DeliverToInboxAsync(_db, messageId),
            () => MarkOutboxDeliveredAsync(_db, messageId),
            () => ProcessAttachFilesAsync(_db, messageId),
        ];

        await using var cleanup = await CleanupTransaction.BeginAsync(_db);
        (await cleanup.TryLockAsync(file)).ShouldBeTrue();
        var pending = false;
        var marked = false;
        Func<Task>[] cleanupSteps =
        [
            async () => pending |= outboxFirst ? await cleanup.PendingInOutboxAsync(file) : await cleanup.PendingInInboxAsync(file),
            async () => pending |= outboxFirst ? await cleanup.PendingInInboxAsync(file) : await cleanup.PendingInOutboxAsync(file),
            async () =>
            {
                marked = !pending && await cleanup.MarkDeletingAsync(file);
                await cleanup.CommitAsync();
            },
        ];

        int c = 0, d = 0;
        foreach (var step in order)
        {
            await (step == 'C' ? cleanupSteps[c++] : delivery[d++])();
        }

        return marked;
    }
}
