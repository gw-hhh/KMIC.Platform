using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Database;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Scheduling;

/// <summary>
/// 滚动发布时旧版本的实例重启，不能把新版本同步的内容改回去
/// </summary>
public sealed class ScheduleBuildGuardTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private const string Added = "BuildingBlocks.AddedLater";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task OlderBuild_LeavesNewerJobsAndTriggersAlone()
    {
        await using (var newer = new ProbeHarness(database.ConnectionString, services: s => s.AddKmicScheduling(
            "BuildingBlocks", x => x.Schedule(Added, "0 0 1 * * ?", run => new StampTicketsCommand(run.ScheduledFireTime)))))
        {
            await newer.Services.GetRequiredService<ScheduleSynchronizer>().SyncAsync("2.0.0+abc", Ct);
        }

        await using var older = new ProbeHarness(database.ConnectionString);
        var synchronizer = older.Services.GetRequiredService<ScheduleSynchronizer>();
        var scheduler = await older.Services.GetRequiredService<ISchedulerFactory>().GetScheduler(Ct);

        await synchronizer.SyncAsync("1.9.0", Ct);

        (await scheduler.GetJobDetail(new JobKey(Added, CommandJob.Group), Ct)).ShouldNotBeNull();
        (await scheduler.GetJobDetail(new JobKey(ProbeHarness.DailyStamp, CommandJob.Group), Ct)).ShouldNotBeNull().JobDataMap["kmic.build"].ShouldBe("2.0.0+abc");

        // 同一个或更新的版本照常同步，删掉代码里已经没有的任务
        await synchronizer.SyncAsync("2.0.0", Ct);

        (await scheduler.GetJobDetail(new JobKey(Added, CommandJob.Group), Ct)).ShouldBeNull();
        (await scheduler.GetJobDetail(new JobKey(ProbeHarness.DailyStamp, CommandJob.Group), Ct)).ShouldNotBeNull().JobDataMap["kmic.build"].ShouldBe("2.0.0");
    }
}
