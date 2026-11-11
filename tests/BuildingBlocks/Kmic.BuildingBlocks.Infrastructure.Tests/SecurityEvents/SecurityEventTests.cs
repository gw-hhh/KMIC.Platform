using System.Text.Json;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Infrastructure.SecurityEvents;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.SecurityEvents;

/// <summary>
/// 每个测试用自己的事件类型，同一个库里的事件互不干扰。写入器走真实的队列；
/// 大部分测试直接调用写库的方法，一个测试启动后台服务验证循环和停机
/// </summary>
public sealed class SecurityEventTests(ClassDatabase<ProbeTemplate> database, ProbeTemplate template) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly string _type = "Test." + Guid.CreateVersion7().ToString("N");
    private readonly List<ProbeHarness> _harnesses = [];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Written_IdentityTimeAndTraceFilledIn()
    {
        var harness = NewHarness();
        await using (var work = harness.Begin(_alice))
        {
            work.Services.GetRequiredService<ISecurityEventWriter>().Write(
                new SecurityEvent(_type, SecuritySeverity.High, "越权访问")
                {
                    ResourceType = "Ticket",
                    ResourceId = Guid.Empty,
                    Details = new Dictionary<string, string>(StringComparer.Ordinal) { ["path"] = "/a" },
                });
        }

        await Flusher(harness).FlushAsync();

        var row = (await RowsAsync()).ShouldHaveSingleItem();
        row.Severity.ShouldBe((short)SecuritySeverity.High);
        row.Reason.ShouldBe("越权访问");
        row.UserId.ShouldBe(_alice.UserId.Value);
        row.RoleId.ShouldBe(_alice.RoleId.Value);
        row.OccurredAt.ShouldBe(harness.Time.GetUtcNow());
        row.TraceId.Length.ShouldBe(32);
        row.ResourceType.ShouldBe("Ticket");
        JsonDocument.Parse(row.Details!).RootElement.GetProperty("path").GetString().ShouldBe("/a");
    }

    [Fact]
    public async Task BackgroundService_WritesAfterTheInterval_AndDrainsOnStop()
    {
        var harness = NewHarness();
        var flusher = Flusher(harness);
        await flusher.StartAsync(Ct);

        await WriteManyAsync(harness, 3);
        await Eventually.UntilAsync(_ => RowsAsync(), rows => rows.Count == 3, "后台循环把事件写进数据库", ct: Ct);

        // 停机时队列里剩下的在退出前写完：停机和写入之间不留间隔
        await WriteManyAsync(harness, 2);
        await flusher.StopAsync(Ct);

        (await RowsAsync()).Count.ShouldBe(5);
    }

    [Fact]
    public async Task ManyEvents_WrittenInBatches()
    {
        var harness = NewHarness(o => o.BatchSize = 50);
        await WriteManyAsync(harness, 230);

        await Flusher(harness).FlushAsync();

        (await RowsAsync()).Count.ShouldBe(230);
    }

    [Fact]
    public async Task QueueFull_NewEventsDroppedAndCounted_WriterNeverThrows()
    {
        var harness = NewHarness(o => o.QueueCapacity = 2);

        await Should.NotThrowAsync(() => WriteManyAsync(harness, 5));

        harness.Services.GetRequiredService<SecurityEventQueue>().Dropped.ShouldBe(3);
        await Flusher(harness).FlushAsync();
        (await RowsAsync()).Count.ShouldBe(2);
    }

    [Fact]
    public async Task LongText_Truncated()
    {
        var harness = NewHarness();
        await using (var work = harness.Begin(_alice))
        {
            work.Services.GetRequiredService<ISecurityEventWriter>().Write(
                new SecurityEvent(_type, SecuritySeverity.Warning, new string('原', 500)) { AttemptedAccount = new string('账', 300) });
        }

        await Flusher(harness).FlushAsync();

        var row = (await RowsAsync()).ShouldHaveSingleItem();
        row.Reason!.Length.ShouldBe(200);
        row.AttemptedAccount!.Length.ShouldBe(100);
    }

    [Fact]
    public async Task RuntimeAccount_InsertsAndReads_CannotUpdateOrDelete()
    {
        await using var connection = new NpgsqlConnection(template.RuntimeConnectionString(database.Name));
        await connection.OpenAsync(Ct);

        await ExecuteAsync(
            connection,
            $"""
            INSERT INTO {SecurityEventTable.Name} ("Id", "OccurredAt", "Type", "Severity", "TraceId")
            VALUES (gen_random_uuid(), now(), '{_type}', 0, '00000000000000000000000000000000')
            """);
        await ExecuteAsync(connection, $"SELECT count(*) FROM {SecurityEventTable.Name}");

        foreach (var sql in new[] { $"UPDATE {SecurityEventTable.Name} SET \"Reason\" = 'x'", $"DELETE FROM {SecurityEventTable.Name}" })
        {
            var error = await Should.ThrowAsync<PostgresException>(() => ExecuteAsync(connection, sql));
            error.SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var harness in _harnesses)
        {
            await harness.DisposeAsync();
        }
    }

    private ProbeHarness NewHarness(Action<SecurityEventOptions>? configure = null)
    {
        var harness = new ProbeHarness(
            database.ConnectionString,
            services: s =>
            {
                s.Configure<SecurityEventOptions>(o =>
                {
                    o.FlushInterval = TimeSpan.FromMilliseconds(50);
                    configure?.Invoke(o);
                });
                s.AddKmicSecurityEvents();

                // 换掉测试默认的记录用写入器，走真实的队列
                s.AddScoped<ISecurityEventWriter, SecurityEventWriter>();
            });
        _harnesses.Add(harness);
        return harness;
    }

    private static SecurityEventFlusher Flusher(ProbeHarness harness) =>
        harness.Services.GetServices<IHostedService>().OfType<SecurityEventFlusher>().Single();

    private async Task WriteManyAsync(ProbeHarness harness, int count)
    {
        await using var work = harness.Begin(_alice);
        var writer = work.Services.GetRequiredService<ISecurityEventWriter>();
        for (var i = 0; i < count; i++)
        {
            writer.Write(new SecurityEvent(_type, SecuritySeverity.Info, null));
        }
    }

    private async Task<List<Row>> RowsAsync()
    {
        await using var command = database.DataSource.CreateCommand(
            $"""
            SELECT "Severity", "Reason", "UserId", "RoleId", "OccurredAt", "TraceId", "ResourceType", "Details"::text, "AttemptedAccount"
            FROM {SecurityEventTable.Name} WHERE "Type" = @type
            """);
        command.Parameters.AddWithValue("type", _type);
        await using var reader = await command.ExecuteReaderAsync(Ct);
        var rows = new List<Row>();
        while (await reader.ReadAsync(Ct))
        {
            rows.Add(new Row(
                reader.GetInt16(0),
                await reader.IsDBNullAsync(1, Ct) ? null : reader.GetString(1),
                await reader.GetFieldValueAsync<Guid?>(2, Ct),
                await reader.GetFieldValueAsync<Guid?>(3, Ct),
                await reader.GetFieldValueAsync<DateTimeOffset>(4, Ct),
                reader.GetString(5),
                await reader.IsDBNullAsync(6, Ct) ? null : reader.GetString(6),
                await reader.IsDBNullAsync(7, Ct) ? null : reader.GetString(7),
                await reader.IsDBNullAsync(8, Ct) ? null : reader.GetString(8)));
        }

        return rows;
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(Ct);
    }

    private sealed record Row(
        short Severity, string? Reason, Guid? UserId, Guid? RoleId, DateTimeOffset OccurredAt, string TraceId, string? ResourceType, string? Details, string? AttemptedAccount);
}
