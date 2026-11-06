using Npgsql;
using Shouldly;
using Xunit;
using static Spike16.Tests.ReadModel;

namespace Spike16.Tests;

// 原型 #17：读模型终态不被迟到的存在快照复活；以及 05-07 第 11 节 R02 补充的读模型场景
public sealed class ReadModelTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    private readonly NpgsqlDataSource _db = fixture.Source;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Late_existence_snapshot_after_purge_does_not_resurrect()
    {
        var id = await CreateSourceAsync(_db, "甲");
        await UpsertAsync(_db, new Snapshot(id, 1, "甲"));

        // 重建扫描先读到存在快照（v1），还没写入；此时来源物理删除，物理删除快照（v2）先到
        var lateScan = (await GetPageAsync(_db, after: null, size: 500)).Single(s => s.Id == id);
        var purgeVersion = await PurgeSourceAsync(_db, id);
        (await ApplyPurgeAsync(_db, id, purgeVersion)).ShouldBeTrue();

        // 迟到的存在快照：不修改，调用方据此记异常
        (await UpsertFromScanAsync(_db, lateScan, Guid.CreateVersion7())).ShouldBeFalse();
        (await UpsertAsync(_db, lateScan)).ShouldBeFalse();

        var row = await RowAsync(_db, id);
        row.IsTerminal.ShouldBeTrue();
        row.TerminalReason.ShouldBe(PurgeEvent);
        row.SourceVersion.ShouldBe(purgeVersion);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Concurrent_purge_and_late_scan_write(bool rowExists)
    {
        var id = await CreateSourceAsync(_db, "乙");
        if (rowExists)
        {
            await UpsertAsync(_db, new Snapshot(id, 1, "乙"));
        }

        var lateScan = new Snapshot(id, 1, "乙");
        var purgeVersion = await PurgeSourceAsync(_db, id);

        // 物理删除快照的处理事务尚未提交时，扫描写入到达：它必须等待，而不是按旧数据判断
        await using var purge = await _db.OpenConnectionAsync(Ct);
        await using var purgeTransaction = await purge.BeginTransactionAsync(Ct);
        (await ApplyPurgeAsync(purge, purgeTransaction, id, purgeVersion)).ShouldBeTrue();

        await using var scan = await _db.OpenConnectionAsync(Ct);
        var scanWrite = UpsertFromScanAsync(scan, null, lateScan, Guid.CreateVersion7());
        await WaitUntilBlockedOnLockAsync(scan.ProcessID);

        await purgeTransaction.CommitAsync(Ct);

        (await scanWrite).ShouldBeFalse();
        (await RowAsync(_db, id)).IsTerminal.ShouldBeTrue();
    }

    [Fact]
    public async Task Scan_write_first_then_purge_still_ends_terminal()
    {
        var id = await CreateSourceAsync(_db, "丙");
        var scanSnapshot = new Snapshot(id, 1, "丙");
        var purgeVersion = await PurgeSourceAsync(_db, id);

        await using var scan = await _db.OpenConnectionAsync(Ct);
        await using var scanTransaction = await scan.BeginTransactionAsync(Ct);
        (await UpsertFromScanAsync(scan, scanTransaction, scanSnapshot, Guid.CreateVersion7())).ShouldBeTrue();

        await using var purge = await _db.OpenConnectionAsync(Ct);
        var purgeWrite = ApplyPurgeAsync(purge, null, id, purgeVersion);
        await WaitUntilBlockedOnLockAsync(purge.ProcessID);
        await scanTransaction.CommitAsync(Ct);

        (await purgeWrite).ShouldBeTrue();
        (await RowAsync(_db, id)).IsTerminal.ShouldBeTrue();
    }

    [Fact]
    public async Task Realtime_writes_only_accept_higher_versions()
    {
        var id = await CreateSourceAsync(_db, "丁");
        (await UpsertAsync(_db, new Snapshot(id, 3, "丁 v3"))).ShouldBeTrue();

        (await UpsertAsync(_db, new Snapshot(id, 2, "丁 v2"))).ShouldBeFalse();
        (await UpsertAsync(_db, new Snapshot(id, 3, "丁 v3 重复"))).ShouldBeFalse();
        (await UpsertAsync(_db, new Snapshot(id, 4, "丁 v4"))).ShouldBeTrue();

        (await RowAsync(_db, id)).Name.ShouldBe("丁 v4");
    }

    [Fact]
    public async Task Rebuild_keeps_unchanged_rows_repairs_content_and_never_lowers_versions()
    {
        await ResetAsync();
        var ids = new List<Guid>();
        for (var i = 0; i < 1000; i++)
        {
            var id = await CreateSourceAsync(_db, $"客户 {i}");
            await UpsertAsync(_db, new Snapshot(id, 1, $"客户 {i}"));
            ids.Add(id);
        }

        // 内容被改坏但版本没变；另有一行读模型已经先收到了更新的实时事件
        await Db.ExecAsync(_db, """UPDATE quotation."CustomerSummary" SET "Name" = '损坏' WHERE "Id" = ANY (@ids)""", ("ids", ids.Take(10).ToArray()));
        await UpsertAsync(_db, new Snapshot(ids[^1], 5, "实时 v5"));

        var outcome = await RebuildAsync(_db, new RebuildOptions());

        outcome.Paused.ShouldBeFalse();
        outcome.Confirmed.ShouldBe(0);
        (await Db.ScalarAsync<long>(_db, """SELECT count(*) FROM quotation."CustomerSummary" WHERE "IsTerminal" OR "Name" = '损坏'""")).ShouldBe(0);
        (await Db.ScalarAsync<long>(_db, """SELECT count(*) FROM quotation."CustomerSummary" WHERE "RebuildRunId" = @run""", ("run", outcome.RunId))).ShouldBe(1000);
        var newer = await RowAsync(_db, ids[^1]);
        newer.SourceVersion.ShouldBe(5);
        newer.Name.ShouldBe("实时 v5");
    }

    [Fact]
    public async Task Rebuild_finish_confirms_missing_and_yields_to_concurrent_purge()
    {
        await ResetAsync();
        var kept = await CreateAndSyncAsync("保留");
        var lostEvent = await CreateAndSyncAsync("物理删除但事件丢失");
        var purgedDuringFinish = await CreateAndSyncAsync("收尾期间收到物理删除");
        await DeleteSourceSilentlyAsync(_db, lostEvent);
        await DeleteSourceSilentlyAsync(_db, purgedDuringFinish);
        var createdAfterScan = Guid.Empty;

        var outcome = await RebuildAsync(_db, new RebuildOptions
        {
            // 扫描越过之后新建，只经实时事件写入
            AfterScan = async () => createdAfterScan = await CreateAndSyncAsync("扫描之后新建"),

            // 选为候选之后、置终态之前，物理删除快照到达
            BeforeConfirmMissing = () => ApplyPurgeAsync(_db, purgedDuringFinish, 2),
        });

        outcome.Confirmed.ShouldBe(1);
        (await RowAsync(_db, kept)).IsTerminal.ShouldBeFalse();
        (await RowAsync(_db, createdAfterScan)).IsTerminal.ShouldBeFalse();
        (await RowAsync(_db, lostEvent)).TerminalReason.ShouldBe(ConfirmedMissing);
        (await RowAsync(_db, purgedDuringFinish)).TerminalReason.ShouldBe(PurgeEvent);
    }

    [Fact]
    public async Task Rebuild_pauses_when_the_source_returns_far_too_few_rows()
    {
        await ResetAsync();
        for (var i = 0; i < 300; i++)
        {
            await CreateAndSyncAsync($"客户 {i}");
        }

        // 来源实现有缺陷：分页与按 ID 读取都返回空
        var outcome = await RebuildAsync(_db, new RebuildOptions
        {
            GetPage = _ => Task.FromResult<IReadOnlyList<Snapshot>>([]),
            GetByIds = _ => Task.FromResult<IReadOnlyDictionary<Guid, Snapshot>>(new Dictionary<Guid, Snapshot>()),
        });

        outcome.Paused.ShouldBeTrue();
        (await Db.ScalarAsync<long>(_db, """SELECT count(*) FROM quotation."CustomerSummary" WHERE "IsTerminal" """)).ShouldBe(0);
    }

    [Fact]
    public async Task Only_one_rebuild_run_at_a_time()
    {
        await ResetAsync();
        await StartRunAsync(_db);

        var error = await Should.ThrowAsync<PostgresException>(() => StartRunAsync(_db));

        error.SqlState.ShouldBe(PostgresErrorCodes.UniqueViolation);
    }

    private async Task<Guid> CreateAndSyncAsync(string name)
    {
        var id = await CreateSourceAsync(_db, name);
        await UpsertAsync(_db, new Snapshot(id, 1, name));
        return id;
    }

    private Task<int> ResetAsync() =>
        Db.ExecAsync(_db, """TRUNCATE customer."Customer", quotation."CustomerSummary", quotation."ReadModelRebuildRun" """);

    private async Task WaitUntilBlockedOnLockAsync(int processId)
    {
        for (var i = 0; i < 100; i++)
        {
            var rows = await Db.QueryAsync(_db, "SELECT wait_event_type FROM pg_stat_activity WHERE pid = @pid", ("pid", processId));
            if (rows.Count == 1 && string.Equals(rows[0][0] as string, "Lock", StringComparison.Ordinal))
            {
                return;
            }

            await Db.ExecAsync(_db, "SELECT pg_sleep(0.05)");
        }

        throw new TimeoutException($"Backend {processId} never waited on a lock.");
    }
}
