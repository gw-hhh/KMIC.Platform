using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shouldly;
using Xunit;

namespace Spike15.Tests;

// R01 H1：命令持有事务连接期间发起嵌套读取，单池在并发达到上限时自锁；拆成事务池 + 读池后不会
public sealed class ConnectionPoolTests(PostgresFixture db) : IClassFixture<PostgresFixture>
{
    private const int TxPoolSize = 5;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Single_pool_locks_up_when_every_command_holds_a_connection_and_reads()
    {
        await using var pool = db.Pool("kmic-api-single", TxPoolSize);
        var allHolding = new AsyncBarrier(TxPoolSize);

        var results = await Task.WhenAll(Enumerable.Range(0, TxPoolSize)
            .Select(_ => Record.ExceptionAsync(() => RunCommandAsync(tx: pool, read: pool, allHolding)).AsTask()));

        // 每个命令都占着池里的一个连接，等第二个连接，直到借连接超时
        results.ShouldAllBe(e => IsPoolExhausted(e));
        Log(results[0]!.ToString());
    }

    [Fact]
    public async Task Split_pools_complete_when_concurrent_commands_reach_the_tx_pool_limit()
    {
        await using var tx = db.Pool("kmic-api-tx", TxPoolSize);
        await using var read = db.Pool("kmic-api-read", maxPoolSize: 1);
        var allHolding = new AsyncBarrier(TxPoolSize);

        await Task.WhenAll(Enumerable.Range(0, TxPoolSize).Select(_ => RunCommandAsync(tx, read, allHolding)));
    }

    [Fact]
    public async Task Split_pools_queue_more_commands_than_connections()
    {
        await using var tx = db.Pool("kmic-api-tx", TxPoolSize, timeoutSeconds: 15);
        await using var read = db.Pool("kmic-api-read", maxPoolSize: 2, timeoutSeconds: 15);

        await Task.WhenAll(Enumerable.Range(0, 40).Select(_ => RunCommandAsync(tx, read, allHolding: null)));
    }

    [Fact]
    public async Task Query_context_releases_its_connection_between_queries()
    {
        await using var read = db.Pool("kmic-api-read", maxPoolSize: 1);
        await using var handler = new ItemsContext(read, readOnly: true);

        // 查询处理器：先查一次，中间经 Contracts 查询另一个模块（另一个上下文、同一个读池），再查一次
        _ = await handler.Items.CountAsync(Ct);
        await using (var contracts = new ItemsContext(read, readOnly: true))
        {
            _ = await contracts.Items.CountAsync(Ct);
        }

        _ = await handler.Items.Where(i => i.Value > 0).ToListAsync(Ct);
    }

    [Fact]
    public async Task Streaming_while_issuing_another_query_holds_the_read_connection()
    {
        await using var read = db.Pool("kmic-api-read", maxPoolSize: 1);
        await SeedAsync(read);
        await using var handler = new ItemsContext(read, readOnly: true);

        var error = await Record.ExceptionAsync(async () =>
        {
            await foreach (var item in handler.Items.AsAsyncEnumerable().WithCancellation(Ct))
            {
                await using var contracts = new ItemsContext(read, readOnly: true);
                _ = await contracts.Items.CountAsync(i => i.Id == item.Id, Ct);
            }
        });

        // 流式枚举期间连接一直被占用，嵌套查询拿不到连接
        IsPoolExhausted(error).ShouldBeTrue(error?.ToString());
    }

    [Fact]
    public async Task Retrying_strategy_does_not_buffer_streamed_queries()
    {
        await using var read = db.Pool("kmic-api-read", maxPoolSize: 1);
        await SeedAsync(read);
        await using var handler = new ItemsContext(read, readOnly: true, QueryRetry.Kmic);

        var error = await Record.ExceptionAsync(async () =>
        {
            await foreach (var item in handler.Items.AsAsyncEnumerable().WithCancellation(Ct))
            {
                await using var lookup = new ItemsContext(read, readOnly: true, QueryRetry.Kmic);
                _ = await lookup.Items.CountAsync(i => i.Id == item.Id, Ct);
            }
        });

        // 启用重试策略后流式查询仍然边读边返回、占着连接（没有先读入内存）
        IsPoolExhausted(error).ShouldBeTrue(error?.ToString());
    }

    [Fact]
    public async Task Ef_default_strategy_keeps_retrying_pool_exhaustion()
    {
        await using var read = db.Pool("kmic-api-read", maxPoolSize: 1, timeoutSeconds: 1);
        await using var holder = await read.OpenConnectionAsync(Ct);
        await using var handler = new ItemsContext(read, readOnly: true, QueryRetry.EfDefault);
        var started = TimeProvider.System.GetTimestamp();

        var error = await Record.ExceptionAsync(() => handler.Items.CountAsync(Ct));

        var elapsed = TimeProvider.System.GetElapsedTime(started);
        Log($"EF 默认策略：{elapsed.TotalSeconds:F1}s 后失败，{error?.GetType().Name}");
        IsPoolExhausted(error).ShouldBeTrue();
        elapsed.ShouldBeGreaterThan(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task Kmic_strategy_fails_fast_on_pool_exhaustion()
    {
        await using var read = db.Pool("kmic-api-read", maxPoolSize: 1, timeoutSeconds: 1);
        await using var holder = await read.OpenConnectionAsync(Ct);
        await using var handler = new ItemsContext(read, readOnly: true, QueryRetry.Kmic);
        var started = TimeProvider.System.GetTimestamp();

        var error = await Record.ExceptionAsync(() => handler.Items.CountAsync(Ct));

        var elapsed = TimeProvider.System.GetElapsedTime(started);
        Log($"框架策略：{elapsed.TotalSeconds:F1}s 后失败，{error?.GetType().Name}");
        QueryRetryStrategy.IsPoolExhausted(error).ShouldBeTrue();
        elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Separate_stream_pool_lets_exports_query_while_streaming()
    {
        await using var stream = db.Pool("kmic-worker-stream", maxPoolSize: 1);
        await using var read = db.Pool("kmic-worker-read", maxPoolSize: 1);
        await SeedAsync(read);
        await using var export = new ItemsContext(stream, readOnly: true, QueryRetry.Kmic);
        var rows = 0;

        // 导出在流式池上逐行读取，期间的其他查询走读池：流式池的连接可以等读池，读池的连接从不等别人，不成环
        await foreach (var item in export.Items.AsAsyncEnumerable().WithCancellation(Ct))
        {
            await using var lookup = new ItemsContext(read, readOnly: true, QueryRetry.Kmic);
            rows += await lookup.Items.CountAsync(i => i.Id == item.Id, Ct);
        }

        rows.ShouldBeGreaterThan(0);
    }

    // 命令：事务行为开启事务（占住事务池的一个连接）→ 写入 → 嵌套读取（IQuerySender / Contracts 查询）→ 提交
    private static async Task RunCommandAsync(NpgsqlDataSource tx, NpgsqlDataSource read, AsyncBarrier? allHolding)
    {
        await using var write = new ItemsContext(tx, readOnly: false);
        await using var transaction = await write.Database.BeginTransactionAsync(Ct);
        write.Items.Add(new Item { Value = 1 });
        await write.SaveChangesAsync(Ct);

        if (allHolding is not null)
        {
            await allHolding.SignalAndWaitAsync(Ct);
        }

        await using (var query = new ItemsContext(read, readOnly: true))
        {
            _ = await query.Items.CountAsync(Ct);
        }

        await transaction.CommitAsync(Ct);
    }

    private static async Task SeedAsync(NpgsqlDataSource source)
    {
        await using var context = new ItemsContext(source, readOnly: false);
        context.Items.AddRange(Enumerable.Range(0, 3).Select(i => new Item { Value = i }));
        await context.SaveChangesAsync(Ct);
    }

    // EF 把借连接超时包装成 InvalidOperationException（“可能是暂时性故障”），原始异常在内层
    private static bool IsPoolExhausted(Exception? error)
    {
        for (var e = error; e is not null; e = e.InnerException)
        {
            if (e is NpgsqlException && e.Message.Contains("connection pool has been exhausted", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static void Log(string text) => TestContext.Current.TestOutputHelper?.WriteLine(text);
}
