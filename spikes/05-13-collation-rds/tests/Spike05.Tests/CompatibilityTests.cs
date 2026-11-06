using Npgsql;
using Shouldly;
using Xunit;

namespace Spike05.Tests;

// 原型 #13：框架依赖的 PG 18 特性在目标实例上是否可用（09 第 3 节表格）。本机容器上的结果只作对照，结论以 RDS 上的运行为准
public sealed class CompatibilityTests(TargetDatabase target) : IClassFixture<TargetDatabase>
{
    private readonly NpgsqlDataSource _db = target.Source;
    private readonly string _schema = target.Schema;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Server_information()
    {
        var version = await Db.ScalarAsync<string>(_db, "SELECT version()");
        var icu = await Db.QueryAsync(_db, "SELECT collversion FROM pg_collation WHERE collprovider = 'i' AND collname = 'und-x-icu'");
        Log($"目标：{(target.IsExternal ? "外部实例" : "本机容器")}；{version}；ICU 排序规则版本：{icu.FirstOrDefault()?[0] ?? "无"}");
        version.ShouldStartWith("PostgreSQL 18");
    }

    [Fact]
    public async Task Icu_case_insensitive_collation_drives_equality_and_unique_index()
    {
        await Db.ExecAsync(_db, $"""
            CREATE COLLATION "{_schema}"."CaseInsensitive" (provider = icu, locale = 'und-u-ks-level2', deterministic = false);
            CREATE TABLE "{_schema}"."Account" ("Id" int GENERATED ALWAYS AS IDENTITY PRIMARY KEY, "Code" varchar(50) COLLATE "{_schema}"."CaseInsensitive" NOT NULL);
            CREATE UNIQUE INDEX ON "{_schema}"."Account" ("Code");
            INSERT INTO "{_schema}"."Account" ("Code") VALUES ('ABC-1'), ('Café');
            """);

        (await Db.ScalarAsync<long>(_db, $"""SELECT count(*) FROM "{_schema}"."Account" WHERE "Code" = 'abc-1'""")).ShouldBe(1);
        (await Db.ScalarAsync<long>(_db, $"""SELECT count(*) FROM "{_schema}"."Account" WHERE "Code" = 'CAFÉ'""")).ShouldBe(1);

        // level2：区分重音，不区分大小写
        (await Db.ScalarAsync<long>(_db, $"""SELECT count(*) FROM "{_schema}"."Account" WHERE "Code" = 'Cafe'""")).ShouldBe(0);

        var duplicate = await Should.ThrowAsync<PostgresException>(() =>
            Db.ExecAsync(_db, $"""INSERT INTO "{_schema}"."Account" ("Code") VALUES ('abc-1')"""));
        duplicate.SqlState.ShouldBe(PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task Pg_trgm_is_available_and_backs_the_contains_text_expression()
    {
        (await Db.ScalarAsync<long>(_db, "SELECT count(*) FROM pg_available_extensions WHERE name = 'pg_trgm'")).ShouldBe(1);

        // 已安装则沿用；未安装则装在本次的 schema 中，结束时随 schema 删除
        var installed = await Db.QueryAsync(_db, "SELECT extnamespace::regnamespace::text FROM pg_extension WHERE extname = 'pg_trgm'");
        var extensionSchema = installed.Count == 1 ? (string)installed[0][0]! : _schema;
        if (installed.Count == 0)
        {
            await Db.ExecAsync(_db, $"""CREATE EXTENSION pg_trgm WITH SCHEMA "{_schema}" """);
        }

        await Db.ExecAsync(_db, $"""
            CREATE TABLE "{_schema}"."Customer" ("Id" int GENERATED ALWAYS AS IDENTITY PRIMARY KEY, "Name" text NOT NULL);
            INSERT INTO "{_schema}"."Customer" ("Name") SELECT '客户' || g || CASE WHEN g % 100 = 0 THEN ' ACME' ELSE '' END FROM generate_series(1, 20000) g;
            CREATE INDEX ON "{_schema}"."Customer" USING gin (("Name" COLLATE "C") {extensionSchema}.gin_trgm_ops);
            ANALYZE "{_schema}"."Customer";
            """);

        await using var connection = await _db.OpenConnectionAsync(Ct);
        await Db.ExecAsync(connection, null, "SET enable_seqscan = off");
        await Db.ExecAsync(connection, null, $"""
            PREPARE contains_text(text) AS
            SELECT "Id" FROM "{_schema}"."Customer"
            WHERE ("Name" COLLATE "C") ILIKE ('%' || replace(replace(replace($1, '\', '\\'), '%', '\%'), '_', '\_') || '%') ESCAPE '\'
            """);
        var plan = string.Join('\n', (await Db.QueryAsync(connection, null, "EXPLAIN EXECUTE contains_text('acme')")).Select(r => (string)r[0]!));
        Log(plan);

        plan.ShouldContain("Bitmap Index Scan");
        (await Db.QueryAsync(connection, null, "EXECUTE contains_text('acme')")).Count.ShouldBe(200);
    }

    [Fact]
    public async Task Uuidv7_is_built_in()
    {
        var rows = await Db.QueryAsync(_db, "SELECT uuidv7(), uuid_extract_version(uuidv7())");

        ((short)rows[0][1]!).ShouldBe((short)7);
        Log($"uuidv7() = {rows[0][0]}");
    }

    [Fact]
    public async Task Notify_is_delivered_only_after_commit()
    {
        var channel = $"kmic_{_schema}";
        await using var listener = await _db.OpenConnectionAsync(Ct);
        var received = new List<string>();
        listener.Notification += (_, e) => received.Add(e.Payload);
        await Db.ExecAsync(listener, null, $"""LISTEN "{channel}" """);

        await using (var sender = await _db.OpenConnectionAsync(Ct))
        {
            await using (var rolledBack = await sender.BeginTransactionAsync(Ct))
            {
                await Db.ExecAsync(sender, rolledBack, "SELECT pg_notify(@c, 'rolled back')", ("c", channel));
                await rolledBack.RollbackAsync(Ct);
            }

            await using var committed = await sender.BeginTransactionAsync(Ct);
            await Db.ExecAsync(sender, committed, "SELECT pg_notify(@c, 'committed')", ("c", channel));
            (await WaitForNotificationAsync(listener, 500)).ShouldBeFalse();
            await committed.CommitAsync(Ct);
        }

        (await WaitForNotificationAsync(listener, 5000)).ShouldBeTrue();
        received.ShouldBe(["committed"]);
    }

    [Fact]
    public async Task Skip_locked_and_advisory_locks_work()
    {
        await Db.ExecAsync(_db, $"""
            CREATE TABLE "{_schema}"."Queue" ("Id" int PRIMARY KEY);
            INSERT INTO "{_schema}"."Queue" VALUES (1), (2), (3);
            """);
        await using var holder = await _db.OpenConnectionAsync(Ct);
        await using var holding = await holder.BeginTransactionAsync(Ct);
        await Db.ExecAsync(holder, holding, $"""SELECT 1 FROM "{_schema}"."Queue" WHERE "Id" = 1 FOR UPDATE""");
        await Db.ExecAsync(holder, holding, "SELECT pg_advisory_xact_lock_shared(42)");

        await using var other = await _db.OpenConnectionAsync(Ct);
        await using var claiming = await other.BeginTransactionAsync(Ct);
        var claimed = await Db.QueryAsync(other, claiming, $"""SELECT "Id" FROM "{_schema}"."Queue" ORDER BY "Id" FOR UPDATE SKIP LOCKED""");
        var exclusive = await Db.QueryAsync(other, claiming, "SELECT pg_try_advisory_xact_lock(42)");

        claimed.Select(r => (int)r[0]!).ShouldBe([2, 3]);
        ((bool)exclusive[0][0]!).ShouldBeFalse();
    }

    // Windows 上 Npgsql 的异步 WaitAsync 不响应超时与取消（Linux 上正常），统一用同步 Wait 放在后台线程
    private static Task<bool> WaitForNotificationAsync(NpgsqlConnection listener, int timeoutMilliseconds) =>
        Task.Run(() => listener.Wait(timeoutMilliseconds), Ct);

    private static void Log(string text) => TestContext.Current.TestOutputHelper?.WriteLine(text);
}
