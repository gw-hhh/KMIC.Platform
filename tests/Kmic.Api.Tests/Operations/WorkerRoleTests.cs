using System.Net;
using System.Text.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Waiting;
using Kmic.Operations.Application;
using Shouldly;

namespace Kmic.Api.Tests.Operations;

/// <summary>
/// 宿主同时跑 Api 与 Worker 两个角色：后台的整条链路——发件箱投递、路由、收件箱处理、调度器执行——在真实宿主里走通
/// </summary>
public sealed class WorkerRoleTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template, new Dictionary<string, string>(StringComparer.Ordinal) { ["Kmic:Roles"] = "Api,Worker" }),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Jobs = "/api/v1/operations/scheduled-jobs";

    private static readonly string[] _manage = [OperationsPermissions.JobsView, OperationsPermissions.JobsManage];

    [Fact]
    public async Task AnAuditedCommand_EndsUpInTheAuditLog_ThroughTheOutboxRouteAndInbox()
    {
        var manager = await LoginAsOperatorAsync("worker-audit-manager", [.. _manage, OperationsPermissions.AuditView]);
        var started = TimeProvider.System.GetUtcNow();

        var changed = await SendAsync(HttpMethod.Put, $"{Jobs}/Identity.PurgeLoginThrottle/override", manager, new { enabled = true, cron = (string?)null, expectedVersion = (long?)null });
        changed.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 审计消息由发件箱的投递程序送到 Operations 的收件箱，再由订阅命令落成一行
        await Eventually.UntilAsync(
            _ => Seed.CountAsync("""SELECT count(*) FROM "Operations"."AuditLog" WHERE "Action" = 'Operations.SetJobOverride'"""),
            count => count >= 1,
            "审计汇总到 Operations 的审计表",
            ct: Ct);
        var from = Uri.EscapeDataString(started.AddMinutes(-1).ToString("O"));
        var to = Uri.EscapeDataString(started.AddMinutes(5).ToString("O"));
        var list = await SendAsync(HttpMethod.Get, $"/api/v1/operations/audit?from={from}&to={to}&action=Operations.SetJobOverride", manager);
        list.StatusCode.ShouldBe(HttpStatusCode.OK);
        var item = (await JsonAsync(list)).GetProperty("items").EnumerateArray().Single();
        item.GetProperty("module").GetString().ShouldBe("Operations");
        item.GetProperty("actorName").GetString().ShouldBe("操作者 worker-audit-manager");
        item.GetProperty("origin").GetString().ShouldBe("Http");
    }

    [Fact]
    public async Task RunningATaskNow_IsExecutedByTheScheduler_AndRecordedAsAManualRun()
    {
        var manager = await LoginAsOperatorAsync("worker-run-manager", _manage);
        const string Name = "Identity.PurgeSessions";

        var triggered = await SendAsync(HttpMethod.Post, $"{Jobs}/{Name}/run", manager);

        triggered.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await Eventually.UntilAsync(
            _ => Seed.CountAsync($"""SELECT count(*) FROM "Operations"."ScheduledJobRun" WHERE "JobName" = '{Name}' AND "Trigger" = 1 AND "Status" = 1"""),
            count => count >= 1,
            "任务被调度器执行，执行记录里是手动触发、成功",
            ct: Ct);
        var runs = await SendAsync(HttpMethod.Get, $"{Jobs}/{Name}/runs", manager);
        var run = (await JsonAsync(runs)).GetProperty("items").EnumerateArray().First();
        run.GetProperty("trigger").GetString().ShouldBe("Manual");
        run.GetProperty("status").GetString().ShouldBe("Succeeded");
        run.GetProperty("triggeredByName").GetString().ShouldBe("操作者 worker-run-manager");
        run.GetProperty("finishedAt").ValueKind.ShouldBe(JsonValueKind.String);
        var list = await SendAsync(HttpMethod.Get, Jobs, manager);
        var job = (await JsonAsync(list)).EnumerateArray().Single(j => string.Equals(j.GetProperty("name").GetString(), Name, StringComparison.Ordinal));
        job.GetProperty("lastRun").GetProperty("trigger").GetString().ShouldBe("Manual");
    }
}
