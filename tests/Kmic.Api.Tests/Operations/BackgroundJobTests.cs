using System.Globalization;
using System.Net;
using System.Text.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Operations.Application;
using Shouldly;

namespace Kmic.Api.Tests.Operations;

/// <summary>
/// 后台作业汇总：跨模块列表与翻页、筛选、我的作业、管理员取消，端到端。作业行直接写进各模块的作业表
/// </summary>
public sealed class BackgroundJobTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/operations";

    private static readonly string[] _view = [OperationsPermissions.JobsView];
    private static readonly string[] _manage = [OperationsPermissions.JobsView, OperationsPermissions.JobsManage];

    private const string Arguments = "{\"secret\":\"筛选条件\"}";

    private readonly DateTimeOffset _base = TimeProvider.System.GetUtcNow().AddHours(-5);

    private async Task<Guid> InsertAsync(string module, string type, int status, DateTimeOffset submitted, Guid? by, string? error = null, string? summary = null)
    {
        var job = Guid.CreateVersion7();
        var submitter = by is { } user ? $"'{user}'" : "NULL";
        var result = summary is null ? "NULL" : $"'{summary}'";
        var errorCode = error is null ? "NULL" : "'Test.Failure'";
        var errorMessage = error is null ? "NULL" : $"'{error}'";
        var started = status >= 1 ? $"'{submitted.AddMinutes(1):O}'" : "NULL";
        var finished = status is 2 or 3 or 5 ? $"'{submitted.AddMinutes(2):O}'" : "NULL";
        await Seed.SqlAsync(
            string.Create(
                CultureInfo.InvariantCulture,
                $"""
                INSERT INTO "{module}"."BackgroundJob" ("Id", "JobType", "SchemaVersion", "Args", "SubmittedBy", "SubmittedAt", "Status", "ProgressDone", "ProgressTotal", "Attempts", "ResultSummary", "ErrorCode", "ErrorMessage", "StartedAt", "FinishedAt")
                VALUES ('{job}', '{type}', 1, '{Arguments}', {submitter}, '{submitted:O}', {status}, 40, 100, 1, {result}, {errorCode}, {errorMessage}, {started}, {finished})
                """));
        return job;
    }

    private async Task<(string Token, Guid UserId)> LoginAsync(string account, IReadOnlyCollection<string> permissions, string displayName)
    {
        var role = await Seed.RoleAsync("作业角色-" + account, permissions);
        var user = await Seed.UserAsync(account, Password, displayName: displayName);
        await Seed.GrantAsync(user, role);
        return ((await LoginTokensAsync(account)).GetProperty("access_token").GetString()!, user);
    }

    private Task<HttpResponseMessage> GetAsync(string token, string path) => SendAsync(HttpMethod.Get, Root + path, token);

    private async Task<List<JsonElement>> WalkAsync(string token, string path, int limit)
    {
        var seen = new List<JsonElement>();
        string? after = null;
        do
        {
            var separator = path.Contains('?', StringComparison.Ordinal) ? "&" : "?";
            var response = await GetAsync(token, $"{path}{separator}limit={limit}" + (after is null ? string.Empty : $"&after={Uri.EscapeDataString(after)}"));
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var page = await JsonAsync(response);
            seen.AddRange(page.GetProperty("items").EnumerateArray());
            after = page.GetProperty("nextCursor").ValueKind == JsonValueKind.Null ? null : page.GetProperty("nextCursor").GetString();
        }
        while (after is not null);

        return seen;
    }

    private static string CodeOf(JsonElement problem) => problem.GetProperty("code").GetString()!;

    [Fact]
    public async Task TheList_MergesModulesNewestFirst_PagesWithoutLosingTiedRows_AndHidesTheArguments()
    {
        var viewer = await LoginAsync("bg-list-viewer", _view, "作业查看员");
        var owner = await Seed.UserAsync("bg-list-owner", Password, displayName: "提交作业的人");
        var tie = _base.AddHours(1);
        var ids = new List<Guid>
        {
            await InsertAsync("Sample", "Sample.Export", status: 2, _base, owner, summary: """{"rows":40}"""),
            await InsertAsync("Configuration", "Configuration.Import", status: 3, _base.AddMinutes(30), owner, error: "导入失败"),
            await InsertAsync("Sample", "Sample.Export", status: 0, tie, owner),
            await InsertAsync("Configuration", "Configuration.Import", status: 1, tie, owner),
            await InsertAsync("Operations", "Operations.ExportAudit", status: 2, tie, null),
            await InsertAsync("Sample", "Sample.Export", status: 5, _base.AddHours(2), owner),
        };

        var all = await WalkAsync(viewer.Token, "/background-jobs?submittedBy=" + owner, limit: 2);
        var single = await WalkAsync(viewer.Token, "/background-jobs", limit: 200);

        all.Count.ShouldBe(5, "提交人筛选下的 5 个");
        all.Select(j => j.GetProperty("id").GetGuid()).Distinct().Count().ShouldBe(5);
        var mine = single.Where(j => ids.Contains(j.GetProperty("id").GetGuid())).ToList();
        mine.Count.ShouldBe(6, "同一时间的三个作业、分属三个模块，翻页时一个也不能漏");
        var times = mine.Select(j => j.GetProperty("submittedAt").GetDateTimeOffset()).ToList();
        times.ShouldBe(times.OrderByDescending(t => t));
        var walked = await WalkAsync(viewer.Token, "/background-jobs", limit: 2);
        walked.Select(j => j.GetProperty("id").GetGuid()).ShouldBe(single.Select(j => j.GetProperty("id").GetGuid()), "每页两条翻完，和一次取完的顺序一样，没有重复也没有遗漏");
        var failed = mine.Single(j => string.Equals(j.GetProperty("status").GetString(), "Failed", StringComparison.Ordinal));
        failed.GetProperty("module").GetString().ShouldBe("Configuration");
        failed.GetProperty("errorCode").GetString().ShouldBe("Test.Failure");
        failed.GetProperty("submittedByName").GetString().ShouldBe("提交作业的人");
        mine.Single(j => j.GetProperty("resultSummary").ValueKind == JsonValueKind.Object).GetProperty("resultSummary").GetProperty("rows").GetInt32().ShouldBe(40);
        mine.Where(j => j.TryGetProperty("args", out _)).ShouldBeEmpty("作业参数不对外");
        JsonSerializer.Serialize(mine).ShouldNotContain("筛选条件");
    }

    [Fact]
    public async Task TheList_FiltersByModuleTypeStatusAndSubmitter_AndNeedsThePermission()
    {
        var viewer = await LoginAsync("bg-filter-viewer", _view, "筛选查看员");
        var plain = await LoginAsync("bg-filter-plain", [], "没有权限的人");
        var owner = Guid.CreateVersion7();
        var queued = await InsertAsync("Sample", "Sample.FilterType", status: 0, _base, owner);
        await InsertAsync("Sample", "Sample.OtherType", status: 2, _base.AddMinutes(1), owner);
        await InsertAsync("Configuration", "Sample.FilterType", status: 0, _base.AddMinutes(2), Guid.CreateVersion7());

        var byType = await WalkAsync(viewer.Token, "/background-jobs?jobType=Sample.FilterType&module=Sample", 50);
        var byStatus = await WalkAsync(viewer.Token, $"/background-jobs?status=Queued&submittedBy={owner}", 50);
        var unknownModule = await WalkAsync(viewer.Token, "/background-jobs?module=NoSuchModule", 50);
        var badStatus = await GetAsync(viewer.Token, "/background-jobs?status=Sleeping");
        var forbidden = await GetAsync(plain.Token, "/background-jobs");

        byType.Select(j => j.GetProperty("id").GetGuid()).ShouldBe([queued]);
        byStatus.Select(j => j.GetProperty("id").GetGuid()).ShouldBe([queued]);
        unknownModule.ShouldBeEmpty();
        badStatus.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await JsonAsync(badStatus)).GetProperty("errors")[0].GetProperty("path").GetString().ShouldBe("status");
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task MyJobs_ShowsOnlyWhatTheCurrentUserSubmitted_AcrossModules()
    {
        var (token, me) = await LoginAsync("bg-mine-user", [], "我");
        var mineSample = await InsertAsync("Sample", "Sample.Export", status: 1, _base, me);
        var mineConfiguration = await InsertAsync("Configuration", "Configuration.Import", status: 2, _base.AddMinutes(5), me);
        await InsertAsync("Sample", "Sample.Export", status: 1, _base.AddMinutes(10), Guid.CreateVersion7());

        var jobs = await WalkAsync(token, "/my-jobs", 1);
        var running = await WalkAsync(token, "/my-jobs?status=Running", 50);

        jobs.Select(j => j.GetProperty("id").GetGuid()).ShouldBe([mineConfiguration, mineSample]);
        jobs.ShouldAllBe(j => j.GetProperty("submittedBy").GetGuid() == me);
        running.Select(j => j.GetProperty("id").GetGuid()).ShouldBe([mineSample]);
        (await GetAsync(token, "/my-jobs?status=Sleeping")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var anonymous = NewClient();
        (await anonymous.GetAsync(Root + "/my-jobs", Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MyJob_ReturnsOneOfMyOwnJobs_AndHidesOthersAndUnknownOnesTheSameWay()
    {
        var (token, me) = await LoginAsync("bg-one-user", [], "我");
        var mine = await InsertAsync("Sample", "Sample.Export", status: 1, _base, me, summary: """{"rows":40}""");
        var others = await InsertAsync("Sample", "Sample.Export", status: 1, _base, Guid.CreateVersion7());
        var system = await InsertAsync("Sample", "Sample.Export", status: 1, _base, null);

        var own = await GetAsync(token, $"/my-jobs/Sample/{mine}");
        var other = await GetAsync(token, $"/my-jobs/Sample/{others}");
        var bySystem = await GetAsync(token, $"/my-jobs/Sample/{system}");
        var unknownJob = await GetAsync(token, $"/my-jobs/Sample/{Guid.CreateVersion7()}");
        var unknownModule = await GetAsync(token, $"/my-jobs/NoSuchModule/{mine}");
        using var anonymous = NewClient();
        var unauthenticated = await anonymous.GetAsync($"{Root}/my-jobs/Sample/{mine}", Ct);

        own.StatusCode.ShouldBe(HttpStatusCode.OK);
        var item = await JsonAsync(own);
        item.GetProperty("id").GetGuid().ShouldBe(mine);
        item.GetProperty("status").GetString().ShouldBe("Running");
        item.GetProperty("progressDone").GetInt64().ShouldBe(40);
        item.GetProperty("submittedByName").GetString().ShouldBe("我");
        item.GetProperty("resultSummary").GetProperty("rows").GetInt32().ShouldBe(40);
        JsonSerializer.Serialize(item).ShouldNotContain("筛选条件");
        foreach (var refused in new[] { other, bySystem, unknownJob, unknownModule })
        {
            refused.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            CodeOf(await JsonAsync(refused)).ShouldBe("Operations.BackgroundJobNotFound", "别人的和不存在的一样，不暴露它存在");
        }

        unauthenticated.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TheSubmitter_CancelsTheirOwnJob_ButNotSomeoneElses_AndNotOneThatIsFinished()
    {
        var (token, me) = await LoginAsync("bg-own-cancel", [], "提交人");
        var queued = await InsertAsync("Sample", "Sample.Export", status: 0, _base, me);
        var running = await InsertAsync("Sample", "Sample.Export", status: 1, _base.AddMinutes(1), me);
        var finished = await InsertAsync("Sample", "Sample.Export", status: 2, _base.AddMinutes(2), me);
        var others = await InsertAsync("Sample", "Sample.Export", status: 0, _base.AddMinutes(3), Guid.CreateVersion7());

        var cancelQueued = await SendAsync(HttpMethod.Post, $"{Root}/my-jobs/Sample/{queued}/cancel", token);
        var cancelRunning = await SendAsync(HttpMethod.Post, $"{Root}/my-jobs/Sample/{running}/cancel", token);
        var cancelFinished = await SendAsync(HttpMethod.Post, $"{Root}/my-jobs/Sample/{finished}/cancel", token);
        var cancelOthers = await SendAsync(HttpMethod.Post, $"{Root}/my-jobs/Sample/{others}/cancel", token);
        var unknownModule = await SendAsync(HttpMethod.Post, $"{Root}/my-jobs/NoSuchModule/{queued}/cancel", token);

        cancelQueued.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        cancelRunning.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        cancelFinished.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        CodeOf(await JsonAsync(cancelFinished)).ShouldBe("Operations.BackgroundJobFinished");
        cancelOthers.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        CodeOf(await JsonAsync(cancelOthers)).ShouldBe("Operations.BackgroundJobNotFound");
        unknownModule.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var states = (await WalkAsync(token, "/my-jobs", 50)).ToDictionary(j => j.GetProperty("id").GetGuid());
        states[queued].GetProperty("status").GetString().ShouldBe("Cancelled");
        states[running].GetProperty("status").GetString().ShouldBe("Cancelling");
        states[running].GetProperty("cancelRequestedBy").GetGuid().ShouldBe(me);
        states[finished].GetProperty("status").GetString().ShouldBe("Succeeded");
        var stillQueued = await Seed.ScalarAsync<short>($"""SELECT "Status" FROM "Sample"."BackgroundJob" WHERE "Id" = '{others}'""");
        stillQueued.ShouldBe((short)0, "别人的作业没有被动过");
    }

    [Fact]
    public async Task AnAdministrator_CancelsAQueuedJobAtOnce_ARunningOneIsMarkedCancelling_AFinishedOneRefuses()
    {
        var manager = await LoginAsync("bg-cancel-manager", _manage, "取消作业的人");
        var viewer = await LoginAsync("bg-cancel-viewer", _view, "只能看的人");
        var queued = await InsertAsync("Sample", "Sample.Export", status: 0, _base, null);
        var running = await InsertAsync("Sample", "Sample.Export", status: 1, _base.AddMinutes(1), null);
        var finished = await InsertAsync("Sample", "Sample.Export", status: 2, _base.AddMinutes(2), null);

        var cancelQueued = await SendAsync(HttpMethod.Post, $"{Root}/background-jobs/Sample/{queued}/cancel", manager.Token);
        var cancelRunning = await SendAsync(HttpMethod.Post, $"{Root}/background-jobs/Sample/{running}/cancel", manager.Token);
        var cancelFinished = await SendAsync(HttpMethod.Post, $"{Root}/background-jobs/Sample/{finished}/cancel", manager.Token);
        var unknownModule = await SendAsync(HttpMethod.Post, $"{Root}/background-jobs/NoSuchModule/{queued}/cancel", manager.Token);
        var unknownJob = await SendAsync(HttpMethod.Post, $"{Root}/background-jobs/Sample/{Guid.CreateVersion7()}/cancel", manager.Token);
        var forbidden = await SendAsync(HttpMethod.Post, $"{Root}/background-jobs/Sample/{running}/cancel", viewer.Token);

        cancelQueued.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        cancelRunning.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        cancelFinished.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        CodeOf(await JsonAsync(cancelFinished)).ShouldBe("Operations.BackgroundJobFinished");
        unknownModule.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        unknownJob.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        CodeOf(await JsonAsync(unknownJob)).ShouldBe("Operations.BackgroundJobNotFound");
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var after = (await WalkAsync(manager.Token, "/background-jobs?module=Sample&jobType=Sample.Export", 50)).ToDictionary(j => j.GetProperty("id").GetGuid());
        after[queued].GetProperty("status").GetString().ShouldBe("Cancelled");
        after[running].GetProperty("status").GetString().ShouldBe("Cancelling");
        after[running].GetProperty("cancelRequestedBy").GetGuid().ShouldBe(manager.UserId);
        after[running].GetProperty("cancelRequestedByName").GetString().ShouldBe("取消作业的人");
        after[finished].GetProperty("status").GetString().ShouldBe("Succeeded");
    }
}
