using System.Text.Json;
using Kmic.BuildingBlocks.Application.Exports;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Jobs;

/// <summary>
/// 测试身份默认只能看自己的单据，导出的结果应当只有提交人自己的
/// </summary>
public sealed class ExportTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private readonly TestIdentity _user = TestIdentity.User("导出人").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(ProbePermissions.Recount));
    private readonly TestIdentity _other = TestIdentity.User("别人").InTree(_org).InOrgUnit(_org["总部"]);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Export_OnlySubmittersRows_OverStreamPool()
    {
        await using var harness = NewHarness();
        var tickets = await AddTicketsAsync(harness, _user, "甲", "乙", "丙");
        await AddTicketsAsync(harness, _other, "别人的");

        var jobId = await SubmitAsync(harness, tickets[0]);
        await harness.Services.RunJobsAsync(Ct);

        var job = await JobAsync(harness, jobId);
        job.Status.ShouldBe(JobStatus.Succeeded);
        JsonDocument.Parse(job.ResultSummary!).RootElement.GetProperty("rows").GetInt64().ShouldBe(3);
        harness.Hooks.Exported.ShouldBe(["甲", "乙", "丙"]);

        await using var pool = database.DataSource.CreateCommand(
            """SELECT count(*) FROM pg_stat_activity WHERE application_name = 'kmic-worker-stream' AND datname = current_database()""");
        ((long)(await pool.ExecuteScalarAsync(Ct))!).ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task OverRowLimit_FailsAsTooLarge()
    {
        await using var harness = NewHarness(o => o.ExportMaxRows = 2);
        var tickets = await AddTicketsAsync(harness, _user, "一", "二", "三");

        var jobId = await SubmitAsync(harness, tickets[0]);
        await harness.Services.RunJobsAsync(Ct);

        (await JobAsync(harness, jobId)).ErrorCode.ShouldBe(CommonErrors.ExportTooLarge.Code);
        harness.Hooks.Exported.ShouldBe(["一", "二"]);
    }

    [Fact]
    public async Task OutsideBackgroundJob_Refused()
    {
        await using var harness = NewHarness();
        await using var work = harness.Begin(_user);

        var reader = work.Services.GetRequiredService<IExportReader<IProbeQueryDb>>();

        await Should.ThrowAsync<InvalidOperationException>(() => reader.ReadAsync(db => db.Tickets, (_, _) => ValueTask.CompletedTask, Ct));
    }

    private ProbeHarness NewHarness(Action<JobOptions>? configure = null)
    {
        var harness = new ProbeHarness(database.ConnectionString, services: s => s.Configure<JobOptions>(o => configure?.Invoke(o)));
        harness.Access.Register(_user);
        return harness;
    }

    private static async Task<List<Ticket>> AddTicketsAsync(ProbeHarness harness, TestIdentity owner, params string[] titles)
    {
        var added = new List<Ticket>();
        foreach (var title in titles)
        {
            added.Add(await harness.AddAsync(owner, Ticket.Create(TicketNumbers.Next(), owner.UserId, _org["总部"], title)));
        }

        return added;
    }

    private async Task<Guid> SubmitAsync(ProbeHarness harness, Ticket ticket)
    {
        await using var work = harness.Begin(_user);
        return (await work.Sender.SendAsync(new StartRecountCommand(ticket.Id, Export: true), Ct)).ShouldBeSuccess();
    }

    private static async Task<BackgroundJobRecord> JobAsync(ProbeHarness harness, Guid id)
    {
        await using var check = harness.BeginAsSystem();
        return await check.Db.Set<BackgroundJobRecord>().AsNoTracking().SingleAsync(j => j.Id == id, Ct);
    }
}
