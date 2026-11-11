using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Scheduling;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Scheduling;

/// <summary>
/// 真正运行调度器的测试换成系统时钟：注入的测试时钟是固定的，调度器永远等不到触发时间
/// </summary>
public sealed class ScheduledRunTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private const string EverySecond = "BuildingBlocks.EverySecond";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly DateTimeOffset _sevenAm = new(2026, 11, 10, 23, 0, 0, TimeSpan.Zero);

    private readonly TestIdentity _user = TestIdentity.User("调度员").InTree(_org).InOrgUnit(_org["总部"]);
    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Fire_Succeeded_RecordHasOutcome()
    {
        var ticket = await _harness.AddAsync(_user, Ticket.Create(TicketNumbers.Next(), _user.UserId, _org["总部"]));
        _harness.Hooks.StampTicket = ticket.Id;

        (await _harness.Services.FireAsync(ProbeHarness.DailyStamp, _sevenAm, Ct)).ShouldBeSuccess().ItemsProcessed.ShouldBe(1);

        var run = await LastRunAsync(ProbeHarness.DailyStamp);
        run.Status.ShouldBe(ScheduledRunStatus.Succeeded);
        run.ScheduledFireTime.ShouldBe(_sevenAm);
        run.Trigger.ShouldBe((short)0);
        run.ItemsProcessed.ShouldBe(1);
        run.Note.ShouldBe("已盖章");
        run.Finished.ShouldBeTrue();
        (await LinesAsync(ticket)).ShouldBe(["定时 23:00"]);
    }

    [Fact]
    public async Task Fire_CommandFails_RolledBackButFailureRecorded()
    {
        var ticket = await _harness.AddAsync(_user, Ticket.Create(TicketNumbers.Next(), _user.UserId, _org["总部"]));
        _harness.Hooks.StampTicket = ticket.Id;
        _harness.Hooks.FailScheduled = true;

        (await _harness.Services.FireAsync(ProbeHarness.DailyStamp, _sevenAm, Ct)).ShouldBeFailure(TicketErrors.Rejected);

        var run = await LastRunAsync(ProbeHarness.DailyStamp);
        run.Status.ShouldBe(ScheduledRunStatus.Failed);
        run.ErrorCode.ShouldBe(TicketErrors.Rejected.Code);
        run.Finished.ShouldBeTrue();
        (await LinesAsync(ticket)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Cluster_TwoInstances_EachFireRunsOnce()
    {
        await using var first = NewWorker(everySecond: true);
        await using var second = NewWorker(everySecond: true);
        await first.StartHostedServicesAsync(Ct);
        await second.StartHostedServicesAsync(Ct);
        try
        {
            await Eventually.UntilAsync(
                async ct => await CountAsync($"""SELECT count(*) FROM {SchedulingTables.Runs} WHERE "JobName" = '{EverySecond}' AND "Status" = 1""", ct) >= 5,
                "每秒的任务执行了 5 次",
                TimeSpan.FromSeconds(30),
                Ct);
        }
        finally
        {
            await first.StopHostedServicesAsync(CancellationToken.None);
            await second.StopHostedServicesAsync(CancellationToken.None);
        }

        (await CountAsync(
            $"""
            SELECT count(*) FROM (SELECT "ScheduledFireTime" FROM {SchedulingTables.Runs}
            WHERE "JobName" = '{EverySecond}' GROUP BY "ScheduledFireTime" HAVING count(*) > 1) duplicated
            """,
            Ct)).ShouldBe(0);
        (first.Hooks.ScheduledFires.Count + second.Hooks.ScheduledFires.Count)
            .ShouldBe((int)await CountAsync($"""SELECT count(*) FROM {SchedulingTables.Runs} WHERE "JobName" = '{EverySecond}'""", Ct));
    }

    [Fact]
    public async Task StandbyInstance_TriggerNow_RunByWorkerInstance()
    {
        await using var worker = NewWorker(everySecond: false);
        await using var standbyHost = new ProbeHarness(database.ConnectionString, services: s => s.AddSingleton(TimeProvider.System));
        await worker.StartHostedServicesAsync(Ct);
        try
        {
            // 没注册执行的实例只写触发请求，由运行调度器的实例执行。
            // 触发时刻取写入实例的时钟，用固定的测试时钟会写成过去的时间，被当成错过的触发
            var standby = standbyHost.Services.GetRequiredService<ScheduleSynchronizer>();
            (await standby.TriggerAsync("BuildingBlocks.Missing", _user.UserId, Ct)).ShouldBeFalse();
            (await standby.TriggerAsync(ProbeHarness.DailyStamp, _user.UserId, Ct)).ShouldBeTrue();

            await Eventually.UntilAsync(
                async ct => await CountAsync(
                    $"""SELECT count(*) FROM {SchedulingTables.Runs} WHERE "JobName" = '{ProbeHarness.DailyStamp}' AND "Trigger" = 1 AND "TriggeredBy" = '{_user.UserId.Value}' AND "Status" = 1""",
                    ct) == 1,
                "手动执行完成",
                TimeSpan.FromSeconds(30),
                Ct);
        }
        finally
        {
            await worker.StopHostedServicesAsync(CancellationToken.None);
        }

        worker.Hooks.ScheduledFires.Count.ShouldBe(1);
        standbyHost.Hooks.ScheduledFires.ShouldBeEmpty();
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private ProbeHarness NewWorker(bool everySecond) => new(database.ConnectionString, services: s =>
    {
        s.AddSingleton(TimeProvider.System);
        if (everySecond)
        {
            s.AddKmicScheduling("BuildingBlocks", x => x.Schedule(EverySecond, "* * * * * ?", run => new StampTicketsCommand(run.ScheduledFireTime)));
        }

        s.AddKmicSchedulerWorker();
    });

    private async Task<RunRow> LastRunAsync(string job)
    {
        await using var command = database.DataSource.CreateCommand(
            $"""
            SELECT "Status", "ScheduledFireTime", "Trigger", "ItemsProcessed", "Note", "ErrorCode", "FinishedAt" IS NOT NULL AND "DurationMs" IS NOT NULL
            FROM {SchedulingTables.Runs} WHERE "JobName" = @job ORDER BY "Id" DESC LIMIT 1
            """);
        command.Parameters.AddWithValue("job", job);
        await using var reader = await command.ExecuteReaderAsync(Ct);
        (await reader.ReadAsync(Ct)).ShouldBeTrue();
        return new RunRow(
            reader.GetInt16(0),
            await reader.GetFieldValueAsync<DateTimeOffset>(1, Ct),
            reader.GetInt16(2),
            await reader.IsDBNullAsync(3, Ct) ? null : reader.GetInt32(3),
            await reader.IsDBNullAsync(4, Ct) ? null : reader.GetString(4),
            await reader.IsDBNullAsync(5, Ct) ? null : reader.GetString(5),
            reader.GetBoolean(6));
    }

    private async Task<long> CountAsync(string sql, CancellationToken ct)
    {
        await using var command = database.DataSource.CreateCommand(sql);
        return (long)(await command.ExecuteScalarAsync(ct))!;
    }

    private async Task<string[]> LinesAsync(Ticket ticket)
    {
        await using var check = _harness.BeginAsSystem();
        return [.. await check.Db.Set<Ticket>().Where(t => t.Id == ticket.Id).SelectMany(t => t.Lines).Select(l => l.Text).ToListAsync(Ct)];
    }

    private sealed record RunRow(short Status, DateTimeOffset ScheduledFireTime, short Trigger, int? ItemsProcessed, string? Note, string? ErrorCode, bool Finished);
}
