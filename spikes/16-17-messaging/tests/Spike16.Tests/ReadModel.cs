using Npgsql;

namespace Spike16.Tests;

// 05-07 第 8.2、8.3 节 R02 补充说明：读模型的三种写入、重建与收尾
internal static class ReadModel
{
    public const short PurgeEvent = 1;
    public const short ConfirmedMissing = 2;

    // ---- 来源模块（customer）

    public static async Task<Guid> CreateSourceAsync(NpgsqlDataSource db, string name)
    {
        var id = Guid.CreateVersion7();
        await Db.ExecAsync(db, """INSERT INTO customer."Customer" ("Id", "Version", "Name") VALUES (@id, 1, @name)""", ("id", id), ("name", name));
        return id;
    }

    public static Task<long> UpdateSourceAsync(NpgsqlDataSource db, Guid id, string name) =>
        Db.ScalarAsync<long>(db, """
            UPDATE customer."Customer" SET "Version" = "Version" + 1, "Name" = @name WHERE "Id" = @id RETURNING "Version"
            """, ("id", id), ("name", name));

    // IPurge 的框架包装：锁住、读出最后版本、物理删除；物理删除快照的版本 = 最后版本 + 1
    public static async Task<long> PurgeSourceAsync(NpgsqlDataSource db, Guid id) =>
        await Db.ScalarAsync<long>(db, """
            WITH last AS (SELECT "Version" FROM customer."Customer" WHERE "Id" = @id FOR UPDATE),
                 gone AS (DELETE FROM customer."Customer" WHERE "Id" = @id)
            SELECT "Version" + 1 FROM last
            """, ("id", id));

    public static Task<int> DeleteSourceSilentlyAsync(NpgsqlDataSource db, Guid id) =>
        Db.ExecAsync(db, """DELETE FROM customer."Customer" WHERE "Id" = @id""", ("id", id));

    public static async Task<IReadOnlyList<Snapshot>> GetPageAsync(NpgsqlDataSource db, Guid? after, int size) =>
        [.. (await Db.QueryAsync(db, """
            SELECT "Id", "Version", "Name" FROM customer."Customer"
            WHERE @after::uuid IS NULL OR "Id" > @after
            ORDER BY "Id" LIMIT @size
            """, ("after", after), ("size", size))).Select(Snapshot.From)];

    public static async Task<IReadOnlyDictionary<Guid, Snapshot>> GetByIdsAsync(NpgsqlDataSource db, IReadOnlyCollection<Guid> ids) =>
        (await Db.QueryAsync(db, """SELECT "Id", "Version", "Name" FROM customer."Customer" WHERE "Id" = ANY (@ids)""", ("ids", ids.ToArray())))
            .Select(Snapshot.From).ToDictionary(s => s.Id);

    // ---- 订阅方模块（quotation）的读模型

    // 实时事件：终态不改；只接受更高版本
    public static async Task<bool> UpsertAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, Snapshot s) =>
        await Db.ExecAsync(connection, transaction, """
            INSERT INTO quotation."CustomerSummary" AS r ("Id", "SourceVersion", "Name") VALUES (@id, @v, @name)
            ON CONFLICT ("Id") DO UPDATE SET "SourceVersion" = EXCLUDED."SourceVersion", "Name" = EXCLUDED."Name", "SyncedAt" = clock_timestamp()
            WHERE NOT r."IsTerminal" AND r."SourceVersion" < EXCLUDED."SourceVersion"
            """, ("id", s.Id), ("v", s.Version), ("name", s.Name)) == 1;

    // 物理删除快照：非终态置终态，版本取该事件的版本；本地没有这一行时直接写入终态行
    public static async Task<bool> ApplyPurgeAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, Guid id, long version) =>
        await Db.ExecAsync(connection, transaction, """
            INSERT INTO quotation."CustomerSummary" AS r ("Id", "SourceVersion", "Name", "IsTerminal", "TerminalReason", "TerminalAt")
            VALUES (@id, @v, '', true, 1, clock_timestamp())
            ON CONFLICT ("Id") DO UPDATE SET "IsTerminal" = true, "TerminalReason" = 1, "TerminalAt" = clock_timestamp(), "SourceVersion" = EXCLUDED."SourceVersion"
            WHERE NOT r."IsTerminal"
            """, ("id", id), ("v", version)) == 1;

    // 重建扫描：终态不改（返回 false，由调用方记异常）；本地版本 ≤ 扫描版本时覆盖全部字段；否则只写轮次
    public static async Task<bool> UpsertFromScanAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, Snapshot s, Guid runId) =>
        await Db.ExecAsync(connection, transaction, """
            INSERT INTO quotation."CustomerSummary" AS r ("Id", "SourceVersion", "Name", "RebuildRunId") VALUES (@id, @v, @name, @run)
            ON CONFLICT ("Id") DO UPDATE SET
                "SourceVersion" = GREATEST(r."SourceVersion", EXCLUDED."SourceVersion"),
                "Name" = CASE WHEN r."SourceVersion" <= EXCLUDED."SourceVersion" THEN EXCLUDED."Name" ELSE r."Name" END,
                "RebuildRunId" = EXCLUDED."RebuildRunId",
                "SyncedAt" = clock_timestamp()
            WHERE NOT r."IsTerminal"
            """, ("id", s.Id), ("v", s.Version), ("name", s.Name), ("run", runId)) == 1;

    public static async Task<bool> UpsertAsync(NpgsqlDataSource db, Snapshot s)
    {
        await using var connection = await db.OpenConnectionAsync(Xunit.TestContext.Current.CancellationToken);
        return await UpsertAsync(connection, null, s);
    }

    public static async Task<bool> ApplyPurgeAsync(NpgsqlDataSource db, Guid id, long version)
    {
        await using var connection = await db.OpenConnectionAsync(Xunit.TestContext.Current.CancellationToken);
        return await ApplyPurgeAsync(connection, null, id, version);
    }

    public static async Task<bool> UpsertFromScanAsync(NpgsqlDataSource db, Snapshot s, Guid runId)
    {
        await using var connection = await db.OpenConnectionAsync(Xunit.TestContext.Current.CancellationToken);
        return await UpsertFromScanAsync(connection, null, s, runId);
    }

    public static async Task<Guid> StartRunAsync(NpgsqlDataSource db)
    {
        var runId = Guid.CreateVersion7();
        await Db.ExecAsync(db, """
            INSERT INTO quotation."ReadModelRebuildRun" ("RunId", "ReadModel", "Status") VALUES (@run, 'Quotation.CustomerSummary', 'Running')
            """, ("run", runId));
        return runId;
    }

    // 完整扫描 + 收尾。options 中的钩子用于在两个步骤之间插入并发事件
    public static async Task<RebuildOutcome> RebuildAsync(NpgsqlDataSource db, RebuildOptions options)
    {
        var runId = await StartRunAsync(db);
        var anomalies = await ScanAsync(db, runId, options);
        if (options.AfterScan is not null)
        {
            await options.AfterScan();
        }

        return await FinishAsync(db, runId, anomalies, options);
    }

    // 返回“终态但源端返回”的异常次数
    private static async Task<int> ScanAsync(NpgsqlDataSource db, Guid runId, RebuildOptions options)
    {
        var anomalies = 0;
        Guid? after = null;
        while (true)
        {
            var page = await (options.GetPage ?? (a => GetPageAsync(db, a, 500)))(after);
            if (page.Count == 0)
            {
                return anomalies;
            }

            foreach (var snapshot in page)
            {
                if (!await UpsertFromScanAsync(db, snapshot, runId))
                {
                    anomalies++;
                }
            }

            after = page[^1].Id;
        }
    }

    // 候选 = 非终态且本轮未扫描到；源端按 ID 确认，未返回的才置终态（条件更新）；超过保护阈值则暂停
    private static async Task<RebuildOutcome> FinishAsync(NpgsqlDataSource db, Guid runId, int anomalies, RebuildOptions options)
    {
        var candidates = (await Db.QueryAsync(db, """
            SELECT "Id", "SourceVersion" FROM quotation."CustomerSummary" WHERE NOT "IsTerminal" AND "RebuildRunId" IS DISTINCT FROM @run
            """, ("run", runId))).ToDictionary(r => (Guid)r[0]!, r => (long)r[1]!);
        var found = await (options.GetByIds ?? (ids => GetByIdsAsync(db, ids)))(candidates.Keys);
        foreach (var snapshot in found.Values)
        {
            await UpsertFromScanAsync(db, snapshot, runId);
        }

        var missing = candidates.Keys.Where(id => !found.ContainsKey(id)).ToList();
        var total = await Db.ScalarAsync<long>(db, """SELECT count(*) FROM quotation."CustomerSummary" WHERE NOT "IsTerminal" """);
        if (missing.Count > Math.Max(options.MaxMissingCount, total * options.MaxMissingRatio))
        {
            await Db.ExecAsync(db, """UPDATE quotation."ReadModelRebuildRun" SET "Status" = 'PendingConfirmation' WHERE "RunId" = @run""", ("run", runId));
            return new RebuildOutcome(runId, Paused: true, Confirmed: 0, anomalies);
        }

        if (options.BeforeConfirmMissing is not null)
        {
            await options.BeforeConfirmMissing();
        }

        var confirmed = 0;
        foreach (var id in missing)
        {
            confirmed += await Db.ExecAsync(db, """
                UPDATE quotation."CustomerSummary" SET "IsTerminal" = true, "TerminalReason" = 2, "TerminalAt" = clock_timestamp()
                WHERE "Id" = @id AND "SourceVersion" = @v AND NOT "IsTerminal"
                """, ("id", id), ("v", candidates[id]));
        }

        await Db.ExecAsync(db, """UPDATE quotation."ReadModelRebuildRun" SET "Status" = 'Completed' WHERE "RunId" = @run""", ("run", runId));
        return new RebuildOutcome(runId, Paused: false, confirmed, anomalies);
    }

    public static async Task<Row> RowAsync(NpgsqlDataSource db, Guid id) =>
        Row.From((await Db.QueryAsync(db, """
            SELECT "SourceVersion", "Name", "IsTerminal", "TerminalReason", "RebuildRunId" FROM quotation."CustomerSummary" WHERE "Id" = @id
            """, ("id", id)))[0]);
}
