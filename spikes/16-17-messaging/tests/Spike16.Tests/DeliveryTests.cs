using Npgsql;
using Shouldly;
using Xunit;
using static Spike16.Tests.Messaging;

namespace Spike16.Tests;

// 原型 #16：05-07 R02 补充说明中投递、清理与失败路径的三个场景，各带一个按原写法的对照组
public sealed class DeliveryTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    private static readonly TimeSpan _shortLease = TimeSpan.FromSeconds(1);

    private readonly NpgsqlDataSource _db = fixture.Source;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    public async Task Late_stale_deliverer_after_takeover_and_cleanup(bool recheck, long expectedEffects)
    {
        var id = await PublishAsync(_db);
        var a = (await ClaimAsync(_db, _shortLease)).Single(m => m.Id == id);

        // A 读出载荷后暂停；租约到期，B 接管并完成投递与处理
        await ExpireLeaseAsync(id);
        var b = (await ClaimAsync(_db, _shortLease)).Single(m => m.Id == id);
        (await DeliverAsync(_db, b, [Quotation])).ShouldBeTrue();
        await DrainInboxAsync(_db, Quotation);

        // 缩短保留期后清理：来源行删除，随后去重凭证删除
        await CleanupAsync(_db, TimeSpan.Zero);
        (await InboxRowsAsync(_db, Quotation, id)).ShouldBe(0);

        // A 恢复，用内存中的载荷写入
        var written = await WriteInboxAsync(_db, a, Quotation, recheck);
        await DrainInboxAsync(_db, Quotation);

        written.ShouldBe(!recheck);
        (await EffectsAsync(_db, Quotation, id)).ShouldBe(expectedEffects);
    }

    [Fact]
    public async Task Recheck_lock_blocks_takeover_mark_and_cleanup_until_the_inbox_write_commits()
    {
        var id = await PublishAsync(_db);
        var a = (await ClaimAsync(_db, _shortLease)).Single(m => m.Id == id);

        await using var connection = await _db.OpenConnectionAsync(Ct);
        await using var transaction = await connection.BeginTransactionAsync(Ct);
        (await RecheckAsync(connection, transaction, a)).ShouldBeTrue();
        await InsertInboxAsync(connection, transaction, a, Quotation);

        // A 的写入尚未提交时租约到期
        await Db.ExecAsync(_db, "SELECT pg_sleep(1.3)");

        // 接管领取跳过被锁的行；标记已投递、清理删除都要等锁
        (await ClaimAsync(_db, _shortLease)).ShouldNotContain(m => m.Id == id);
        (await LockWaitAsync("""UPDATE customer."OutboxMessage" SET "Status" = 2 WHERE "Id" = @id""", id)).ShouldBeTrue();
        (await LockWaitAsync("""DELETE FROM customer."OutboxMessage" WHERE "Id" = @id""", id)).ShouldBeTrue();

        await transaction.CommitAsync(Ct);
        (await MarkDeliveredAsync(_db, a)).ShouldBe(1);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    public async Task Partial_delivery_dead_letter_then_retry_runs_each_subscriber_once(bool retentionContract, long expectedQuotationEffects)
    {
        var id = await PublishAsync(_db);

        // 写入 quotation 成功；写入 orders 失败，来源行进入死信
        var first = (await ClaimAsync(_db, _shortLease)).Single(m => m.Id == id);
        (await DeliverAsync(_db, first, [Quotation, Orders], failingSubscriber: Orders)).ShouldBeFalse();
        await DrainInboxAsync(_db, Quotation);
        (await Db.ScalarAsync<short>(_db, """SELECT "Status" FROM customer."OutboxMessage" WHERE "Id" = @id""", ("id", id))).ShouldBe((short)3);

        // 已处理行超过保留期
        await CleanupAsync(_db, TimeSpan.Zero, retentionContract);
        (await InboxRowsAsync(_db, Quotation, id)).ShouldBe(retentionContract ? 1 : 0);

        // 运维重试死信，故障已排除
        await RetryDeadLetterAsync(_db, id);
        var retry = (await ClaimAsync(_db, _shortLease)).Single(m => m.Id == id);
        (await DeliverAsync(_db, retry, [Quotation, Orders])).ShouldBeTrue();
        await DrainInboxAsync(_db, Quotation);
        await DrainInboxAsync(_db, Orders);

        (await EffectsAsync(_db, Quotation, id)).ShouldBe(expectedQuotationEffects);
        (await EffectsAsync(_db, Orders, id)).ShouldBe(1);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    public async Task Failure_recorded_after_a_lost_commit_confirmation(bool conditional, long expectedEffects)
    {
        var id = await PublishAsync(_db);
        var message = (await ClaimAsync(_db, _shortLease)).Single(m => m.Id == id);
        await DeliverAsync(_db, message, [Quotation]);
        var row = (await ClaimInboxAsync(_db, Quotation)).Single(r => r.MessageId == id);

        // 处理事务已提交，但处理程序没有收到确认（连接在返回前断开），转入失败路径
        (await ProcessAsync(_db, Quotation, row)).ShouldBeTrue();
        var affected = await RecordProcessingFailureAsync(_db, Quotation, row, conditional);

        if (conditional)
        {
            // 影响 0 行：重新读取再分类，读到“已处理”即结果已提交，不改状态、不重新执行
            affected.ShouldBe(0);
            (await InboxStatusAsync(_db, Quotation, row.Id)).ShouldBe((short)2);
        }

        await DrainInboxAsync(_db, Quotation);
        (await EffectsAsync(_db, Quotation, id)).ShouldBe(expectedEffects);
    }

    private Task<int> ExpireLeaseAsync(Guid id) =>
        Db.ExecAsync(_db, """UPDATE customer."OutboxMessage" SET "LeaseUntil" = clock_timestamp() - interval '1 second' WHERE "Id" = @id""", ("id", id));

    // 在 500 毫秒的锁等待上限内执行；因等锁超时返回 true
    private async Task<bool> LockWaitAsync(string sql, Guid id)
    {
        await using var connection = await _db.OpenConnectionAsync(Ct);
        await using var transaction = await connection.BeginTransactionAsync(Ct);
        await Db.ExecAsync(connection, transaction, "SET LOCAL lock_timeout = '500ms'");
        try
        {
            await Db.ExecAsync(connection, transaction, sql, ("id", id));
            return false;
        }
        catch (PostgresException e) when (string.Equals(e.SqlState, PostgresErrorCodes.LockNotAvailable, StringComparison.Ordinal))
        {
            return true;
        }
        finally
        {
            await transaction.RollbackAsync(Ct);
        }
    }
}
