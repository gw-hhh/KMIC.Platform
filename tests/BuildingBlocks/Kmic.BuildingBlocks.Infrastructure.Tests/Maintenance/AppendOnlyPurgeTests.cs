using Kmic.BuildingBlocks.Infrastructure.Maintenance;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Maintenance;

/// <summary>
/// 用库的管理账号模拟迁移账号：运行账号对这些表没有 DELETE 权限（见 MigrationTests）
/// </summary>
public sealed class AppendOnlyPurgeTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private static readonly DateTimeOffset _cutoff = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task OnlyRowsBeforeTheCutoff_Deleted_InBatches()
    {
        var old = new[] { _cutoff.AddDays(-1), _cutoff.AddDays(-30), _cutoff.AddYears(-1), _cutoff.AddMonths(-5), _cutoff.AddMilliseconds(-1) };
        var kept = new[] { _cutoff, _cutoff.AddDays(1) };
        foreach (var at in old.Concat(kept))
        {
            await InsertAsync(at);
        }

        var deleted = await AppendOnlyPurge.RunAsync(database.DataSource, AppendOnlyPurge.SecurityEvent, _cutoff, batchSize: 2, Ct);

        deleted.ShouldBe(old.Length);
        await using var count = database.DataSource.CreateCommand("""SELECT count(*) FROM "Identity"."SecurityEvent" """);
        (await count.ExecuteScalarAsync(Ct)).ShouldBe((long)kept.Length);
    }

    [Fact]
    public async Task TableOfAModuleNotBuiltYet_Skipped()
    {
        (await AppendOnlyPurge.RunAsync(database.DataSource, AppendOnlyPurge.AuditLog, _cutoff, 100, Ct)).ShouldBeNull();
    }

    [Fact]
    public async Task OtherTables_Refused()
    {
        var thrown = await Should.ThrowAsync<ArgumentException>(() => AppendOnlyPurge.RunAsync(database.DataSource, "Probe.Ticket", _cutoff, 100, Ct));

        thrown.Message.ShouldContain("不是可以清理的表");
    }

    private async Task InsertAsync(DateTimeOffset at)
    {
        await using var command = database.DataSource.CreateCommand(
            """
            INSERT INTO "Identity"."SecurityEvent" ("Id", "OccurredAt", "Type", "Severity", "TraceId")
            VALUES (gen_random_uuid(), @at, 'Test.Purge', 0, '00000000000000000000000000000000')
            """);
        command.Parameters.AddWithValue("at", at);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
