using System.Globalization;
using System.Net;
using System.Text.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Operations.Application;
using Shouldly;

namespace Kmic.Api.Tests.Operations;

/// <summary>
/// 定时任务管理：列表、预览 Cron、改开关与执行时间、恢复默认、立即执行、执行记录，端到端
/// </summary>
public sealed class ScheduledJobTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/operations/scheduled-jobs";

    private static readonly string[] _view = [OperationsPermissions.JobsView];
    private static readonly string[] _manage = [OperationsPermissions.JobsView, OperationsPermissions.JobsManage];

    private Task<HttpResponseMessage> CallAsync(HttpMethod method, string path, string token, object? body = null) =>
        SendAsync(method, Root + path, token, body);

    private async Task<JsonElement> JobAsync(string token, string name)
    {
        var response = await CallAsync(HttpMethod.Get, string.Empty, token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await JsonAsync(response)).EnumerateArray().Single(j => string.Equals(j.GetProperty("name").GetString(), name, StringComparison.Ordinal));
    }

    private static async Task<string> CodeOfAsync(HttpResponseMessage response) => (await JsonAsync(response)).GetProperty("code").GetString()!;

    private static string[] Actions(JsonElement job) => [.. job.GetProperty("allowedActions").EnumerateArray().Select(a => a.GetString()!)];

    [Fact]
    public async Task TheList_ShowsDeclaredTasksWithDefaultsAndTheStateOfTheScheduler()
    {
        var viewer = await LoginAsOperatorAsync("job-list-viewer", _view);
        var manager = await LoginAsOperatorAsync("job-list-manager", _manage);
        var plain = await LoginAsOperatorAsync("job-list-plain", []);

        var job = await JobAsync(viewer, "Identity.PurgeRetiredPermissions");
        var asManager = await JobAsync(manager, "Identity.PurgeRetiredPermissions");
        var forbidden = await CallAsync(HttpMethod.Get, string.Empty, plain);

        job.GetProperty("module").GetString().ShouldBe("Identity");
        job.GetProperty("displayName").GetString().ShouldNotBeNullOrWhiteSpace();
        job.GetProperty("isDeclared").GetBoolean().ShouldBeTrue();
        job.GetProperty("defaultCron").GetString().ShouldBe("0 0 5 ? * SUN");
        job.GetProperty("cron").GetString().ShouldBe("0 0 5 ? * SUN");
        job.GetProperty("activeCron").GetString().ShouldBe("0 0 5 ? * SUN");
        job.GetProperty("enabled").GetBoolean().ShouldBeTrue();
        job.GetProperty("inSync").GetBoolean().ShouldBeTrue();
        job.GetProperty("nextFireTime").ValueKind.ShouldBe(JsonValueKind.String);
        job.GetProperty("overrideVersion").ValueKind.ShouldBe(JsonValueKind.Null);
        Actions(job).ShouldBeEmpty();
        Actions(asManager).ShouldBe(["Edit", "RunNow"]);
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var all = (await JsonAsync(await CallAsync(HttpMethod.Get, string.Empty, viewer))).EnumerateArray().ToList();
        all.Where(j => string.Equals(j.GetProperty("module").GetString(), "Framework", StringComparison.Ordinal)).ShouldNotBeEmpty();
    }

    [Fact]
    public async Task PreviewingACron_GivesTheNextFiveFires_AndRefusesWhatCannotBeUsed()
    {
        var viewer = await LoginAsOperatorAsync("job-preview-viewer", _view);
        var plain = await LoginAsOperatorAsync("job-preview-plain", []);

        var ok = await CallAsync(HttpMethod.Get, "/cron-preview?cron=" + Uri.EscapeDataString("0 0 7 * * ?"), viewer);
        var notCron = await CallAsync(HttpMethod.Get, "/cron-preview?cron=" + Uri.EscapeDataString("每天七点"), viewer);
        var tooOften = await CallAsync(HttpMethod.Get, "/cron-preview?cron=" + Uri.EscapeDataString("0/30 * * * * ?"), viewer);
        var never = await CallAsync(HttpMethod.Get, "/cron-preview?cron=" + Uri.EscapeDataString("0 0 0 1 1 ? 2000"), viewer);
        var forbidden = await CallAsync(HttpMethod.Get, "/cron-preview?cron=" + Uri.EscapeDataString("0 0 7 * * ?"), plain);

        ok.StatusCode.ShouldBe(HttpStatusCode.OK);
        var times = (await JsonAsync(ok)).GetProperty("nextFireTimes").EnumerateArray().Select(t => t.GetDateTimeOffset()).ToList();
        times.Count.ShouldBe(5);
        times.ShouldBe(times.Order());
        times.ShouldAllBe(t => t.UtcDateTime.TimeOfDay == TimeSpan.FromHours(23), "业务时区是东八区，早上 7 点是前一天 23 点（UTC）");
        foreach (var refused in new[] { notCron, tooOften, never })
        {
            refused.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            (await CodeOfAsync(refused)).ShouldBe("Operations.CronInvalid");
            (await JsonAsync(refused)).GetProperty("errors")[0].GetProperty("path").GetString().ShouldBe("cron");
        }

        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DisablingAndMovingATask_ReachesTheSchedulerAtOnce_AndRestoreBringsTheDefaultBack()
    {
        var manager = await LoginAsOperatorAsync("job-edit-manager", _manage);
        const string Name = "Identity.PurgeSessions";

        var disabled = await CallAsync(HttpMethod.Put, $"/{Name}/override", manager, new { enabled = false, cron = (string?)null, expectedVersion = (long?)null });

        disabled.StatusCode.ShouldBe(HttpStatusCode.OK);
        var version = (await JsonAsync(disabled)).GetProperty("version").GetInt64();
        var stopped = await JobAsync(manager, Name);
        stopped.GetProperty("enabled").GetBoolean().ShouldBeFalse();
        stopped.GetProperty("overrideEnabled").GetBoolean().ShouldBeFalse();
        stopped.GetProperty("activeCron").ValueKind.ShouldBe(JsonValueKind.Null);
        stopped.GetProperty("nextFireTime").ValueKind.ShouldBe(JsonValueKind.Null);
        stopped.GetProperty("inSync").GetBoolean().ShouldBeTrue();
        stopped.GetProperty("overrideVersion").GetInt64().ShouldBe(version);
        Actions(stopped).ShouldBe(["Edit", "RestoreDefault", "RunNow"]);

        var moved = await CallAsync(HttpMethod.Put, $"/{Name}/override", manager, new { enabled = (bool?)null, cron = "0 15 4 * * ?", expectedVersion = version });

        moved.StatusCode.ShouldBe(HttpStatusCode.OK);
        var running = await JobAsync(manager, Name);
        running.GetProperty("enabled").GetBoolean().ShouldBeTrue();
        running.GetProperty("cron").GetString().ShouldBe("0 15 4 * * ?");
        running.GetProperty("activeCron").GetString().ShouldBe("0 15 4 * * ?");
        running.GetProperty("defaultCron").GetString().ShouldBe("0 30 3 * * ?");
        running.GetProperty("inSync").GetBoolean().ShouldBeTrue();
        running.GetProperty("nextFireTime").GetDateTimeOffset().UtcDateTime.TimeOfDay.ShouldBe(new TimeSpan(20, 15, 0), "东八区 04:15 是前一天 20:15（UTC）");

        var restored = await CallAsync(HttpMethod.Post, $"/{Name}/restore-default", manager, new { expectedVersion = (await JsonAsync(moved)).GetProperty("version").GetInt64() });

        restored.StatusCode.ShouldBe(HttpStatusCode.OK);
        var back = await JobAsync(manager, Name);
        back.GetProperty("cron").GetString().ShouldBe("0 30 3 * * ?");
        back.GetProperty("activeCron").GetString().ShouldBe("0 30 3 * * ?");
        back.GetProperty("overrideEnabled").ValueKind.ShouldBe(JsonValueKind.Null);
        back.GetProperty("overrideCron").ValueKind.ShouldBe(JsonValueKind.Null);
        back.GetProperty("overrideVersion").ValueKind.ShouldBe(JsonValueKind.Number, "设置行留着，便于追溯");
        Actions(back).ShouldBe(["Edit", "RunNow"]);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Operations"."ScheduledJobSetting" WHERE "JobName" = '{Name}'""")).ShouldBe(1);
    }

    [Fact]
    public async Task ChangingATask_RefusesABadCron_AnUnknownTask_AStaleVersion_AndAMissingPermission()
    {
        var manager = await LoginAsOperatorAsync("job-refuse-manager", _manage);
        var viewer = await LoginAsOperatorAsync("job-refuse-viewer", _view);
        const string Name = "Identity.PurgeLoginThrottle";

        var badCron = await CallAsync(HttpMethod.Put, $"/{Name}/override", manager, new { enabled = (bool?)null, cron = "0/10 * * * * ?", expectedVersion = (long?)null });
        var unknown = await CallAsync(HttpMethod.Put, "/Identity.NoSuchJob/override", manager, new { enabled = false, cron = (string?)null, expectedVersion = (long?)null });
        var forbidden = await CallAsync(HttpMethod.Put, $"/{Name}/override", viewer, new { enabled = false, cron = (string?)null, expectedVersion = (long?)null });
        var first = await CallAsync(HttpMethod.Put, $"/{Name}/override", manager, new { enabled = true, cron = (string?)null, expectedVersion = (long?)null });
        var withoutVersion = await CallAsync(HttpMethod.Put, $"/{Name}/override", manager, new { enabled = false, cron = (string?)null, expectedVersion = (long?)null });
        var stale = await CallAsync(HttpMethod.Put, $"/{Name}/override", manager, new { enabled = false, cron = (string?)null, expectedVersion = 999L });
        var neverChanged = await CallAsync(HttpMethod.Post, "/Identity.PurgeRetiredPermissions/restore-default", manager, new { expectedVersion = 0L });

        badCron.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await CodeOfAsync(badCron)).ShouldBe("Operations.CronInvalid");
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeOfAsync(unknown)).ShouldBe("Operations.JobNotFound");
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        withoutVersion.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(withoutVersion)).ShouldBe("Common.VersionConflict");
        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        neverChanged.StatusCode.ShouldBe(HttpStatusCode.Conflict, "没有设置行，调用方带的版本对不上");
        (await JobAsync(manager, Name)).GetProperty("overrideEnabled").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task ASettingWhoseTaskIsGoneFromTheCode_IsListedAsRemoved_AndCannotBeChanged()
    {
        var manager = await LoginAsOperatorAsync("job-gone-manager", _manage);
        await Seed.SqlAsync(
            $"""
            INSERT INTO "Operations"."ScheduledJobSetting" ("Id", "JobName", "Enabled", "Cron", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            VALUES ('{Guid.CreateVersion7()}', 'Identity.RemovedLongAgo', false, NULL, now(), '00000000-0000-0000-0000-000000000001', 0)
            """);

        var job = await JobAsync(manager, "Identity.RemovedLongAgo");
        var change = await CallAsync(HttpMethod.Put, "/Identity.RemovedLongAgo/override", manager, new { enabled = true, cron = (string?)null, expectedVersion = 0L });

        job.GetProperty("isDeclared").GetBoolean().ShouldBeFalse();
        job.GetProperty("defaultCron").ValueKind.ShouldBe(JsonValueKind.Null);
        job.GetProperty("overrideEnabled").GetBoolean().ShouldBeFalse();
        Actions(job).ShouldBeEmpty();
        change.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RunningATaskNow_HandsItToTheScheduler_ForAnyDeclaredTask()
    {
        var manager = await LoginAsOperatorAsync("job-run-manager", _manage);
        var viewer = await LoginAsOperatorAsync("job-run-viewer", _view);
        const string Name = "Identity.PurgeSessions";

        var triggered = await CallAsync(HttpMethod.Post, $"/{Name}/run", manager);
        var unknown = await CallAsync(HttpMethod.Post, "/Identity.NoSuchJob/run", manager);
        var forbidden = await CallAsync(HttpMethod.Post, $"/{Name}/run", viewer);

        triggered.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeOfAsync(unknown)).ShouldBe("Operations.JobNotFound");
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        // 这个测试宿主不运行调度器，触发请求只是写进了调度库
        (await Seed.CountAsync($"""SELECT count(*) FROM "Operations".qrtz_triggers WHERE job_name = '{Name}' AND job_group = 'Kmic' AND trigger_type = 'SIMPLE'""")).ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task TheRunHistory_PagesNewestFirst_ShowsWhoRanItByHand_AndKeepsTasksApart()
    {
        var viewer = await LoginAsOperatorAsync("job-runs-viewer", _view);
        var plain = await LoginAsOperatorAsync("job-runs-plain", []);
        var operatorId = await Seed.UserAsync("job-runs-hand", Password, displayName: "手动执行的人");
        const string Name = "Identity.History";
        var start = TimeProvider.System.GetUtcNow().AddHours(-10);
        for (var i = 0; i < 5; i++)
        {
            await InsertRunAsync(Name, start.AddHours(i), status: i == 4 ? 2 : 1, trigger: i == 3 ? 1 : 0, by: i == 3 ? operatorId : null);
        }

        await InsertRunAsync("Identity.Other", start, status: 1, trigger: 0, by: null);

        var seen = new List<JsonElement>();
        string? after = null;
        do
        {
            var response = await CallAsync(HttpMethod.Get, $"/{Name}/runs?limit=2" + (after is null ? string.Empty : $"&after={Uri.EscapeDataString(after)}"), viewer);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var page = await JsonAsync(response);
            seen.AddRange(page.GetProperty("items").EnumerateArray());
            after = page.GetProperty("nextCursor").ValueKind == JsonValueKind.Null ? null : page.GetProperty("nextCursor").GetString();
        }
        while (after is not null);

        seen.Count.ShouldBe(5);
        seen.Select(r => r.GetProperty("startedAt").GetDateTimeOffset()).ShouldBe(seen.Select(r => r.GetProperty("startedAt").GetDateTimeOffset()).OrderByDescending(t => t));
        seen.ShouldAllBe(r => string.Equals(r.GetProperty("jobName").GetString(), Name, StringComparison.Ordinal));
        seen[0].GetProperty("status").GetString().ShouldBe("Failed");
        seen[0].GetProperty("errorCode").GetString().ShouldBe("Test.Failure");
        seen[1].GetProperty("trigger").GetString().ShouldBe("Manual");
        seen[1].GetProperty("triggeredByName").GetString().ShouldBe("手动执行的人");
        seen[2].GetProperty("trigger").GetString().ShouldBe("Schedule");
        seen[2].GetProperty("status").GetString().ShouldBe("Succeeded");
        seen[2].GetProperty("itemsProcessed").GetInt32().ShouldBe(7);
        (await CallAsync(HttpMethod.Get, $"/{Name}/runs", plain)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CallAsync(HttpMethod.Get, $"/{Name}/runs?after=nonsense", viewer)).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TheListShowsTheLastRunOfEachTask()
    {
        var viewer = await LoginAsOperatorAsync("job-last-viewer", _view);
        const string Name = "Identity.PurgeRetiredPermissions";
        var now = TimeProvider.System.GetUtcNow();
        await InsertRunAsync(Name, now.AddHours(-3), status: 1, trigger: 0, by: null);
        await InsertRunAsync(Name, now.AddHours(-1), status: 2, trigger: 2, by: null);

        var job = await JobAsync(viewer, Name);

        var last = job.GetProperty("lastRun");
        last.GetProperty("status").GetString().ShouldBe("Failed");
        last.GetProperty("trigger").GetString().ShouldBe("Misfire");
        last.GetProperty("errorCode").GetString().ShouldBe("Test.Failure");
    }

    private Task InsertRunAsync(string job, DateTimeOffset started, int status, int trigger, Guid? by) =>
        Seed.SqlAsync(
            string.Create(
                CultureInfo.InvariantCulture,
                $"""
                INSERT INTO "Operations"."ScheduledJobRun" ("Id", "JobName", "ScheduledFireTime", "ActualFireTime", "Trigger", "TriggeredBy", "StartedAt", "FinishedAt", "DurationMs", "Status", "ItemsProcessed", "Note", "ErrorCode", "ErrorMessage", "InstanceId")
                VALUES ('{Guid.CreateVersion7()}', '{job}', '{started:O}', '{started:O}', {trigger}, {(by is { } user ? $"'{user}'" : "NULL")}, '{started:O}', '{started.AddSeconds(2):O}', 2000, {status}, 7, '备注',
                        {(status == 2 ? "'Test.Failure'" : "NULL")}, {(status == 2 ? "'测试里造的失败'" : "NULL")}, 'instance-1')
                """));
}
