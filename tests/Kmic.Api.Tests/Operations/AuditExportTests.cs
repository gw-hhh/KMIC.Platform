using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Operations.Application;
using Kmic.Operations.Application.Audit.Record;
using Shouldly;

namespace Kmic.Api.Tests.Operations;

/// <summary>
/// 审计导出：提交后台作业，Worker 把审计记录写成 CSV 交给 Files，登记完成后提交人下载，端到端
/// </summary>
/// <remarks>
/// 宿主同时跑 Api 和 Worker；导出的行数上限调成 3，方便测超限
/// </remarks>
public sealed class AuditExportTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(
        database,
        template,
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Kmic:Roles"] = "Api,Worker",
            ["Kmic:Jobs:ExportMaxRows"] = "3",
            ["Files:Storage:LocalRoot"] = _storageRoot,
        }),
    IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/operations/audit/exports";
    private const string EmptyArgs = "{}";

    private static readonly string _storageRoot = Path.Combine(Path.GetTempPath(), "kmic-audit-export-tests-" + Guid.CreateVersion7().ToString("N"));
    private static readonly string[] _export = [OperationsPermissions.AuditExport];
    private static readonly string[] _exportAndManage = [OperationsPermissions.AuditExport, OperationsPermissions.JobsManage];

    private readonly DateTimeOffset _now = TimeProvider.System.GetUtcNow();

    private static AuditRecorded Message(DateTimeOffset at, string module, string action, Guid? actor = null) =>
        new()
        {
            AuditId = Guid.CreateVersion7(),
            OccurredAt = at,
            Module = module,
            Action = action,
            Origin = "Http",
            ActorUserId = actor,
            ClientIp = "10.1.2.3",
            TraceId = "0123456789abcdef0123456789abcdef",
            Targets = [new AuditTarget("Specimen", Guid.CreateVersion7())],
            Changes = [],
            Details = new Dictionary<string, string>(StringComparer.Ordinal),
        };

    private async Task RecordAsync(AuditRecorded message)
    {
        await using var scope = Factory.Services.AsSystem(CallOrigin.Inbox);
        (await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new RecordAuditCommand(message), Ct)).ShouldBeSuccess();
    }

    private object Request(string module, string? action = null) => new
    {
        from = _now.AddDays(-1),
        to = _now.AddDays(1),
        module,
        action,
    };

    private async Task<Guid> SubmitAsync(string token, object body)
    {
        var response = await SendAsync(HttpMethod.Post, Root, token, body);
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        var accepted = await JsonAsync(response);
        var jobId = accepted.GetProperty("jobId").GetGuid();
        response.Headers.Location!.ToString().ShouldBe($"{Root}/{jobId}/file");
        return jobId;
    }

    // 作业要先跑完，文件再经发件箱、路由、收件箱登记，所以要等到不再是“还没好”的 409
    // 半秒问一次：接口有限流，等待工具默认每 50 毫秒一次会被限流
    private async Task<HttpResponseMessage> DownloadWhenReadyAsync(string token, Guid jobId)
    {
        var deadline = TimeProvider.System.GetUtcNow().AddSeconds(40);
        while (true)
        {
            var response = await SendAsync(HttpMethod.Get, $"{Root}/{jobId}/file", token);
            if (response.StatusCode != HttpStatusCode.Conflict)
            {
                return response;
            }

            if (TimeProvider.System.GetUtcNow() > deadline)
            {
                var job = await Seed.ScalarAsync<string>(
                    $"""SELECT "Status"::text || ' ' || coalesce("ErrorCode", '') || ' ' || coalesce("ErrorMessage", '') || ' ' || coalesce("ResultFileId"::text, '') FROM "Operations"."BackgroundJob" WHERE "Id" = '{jobId}'""");
                var file = await Seed.ScalarAsync<string>(
                    $"""SELECT "Status" FROM "Files"."StoredFile" WHERE "Id" = (SELECT "ResultFileId" FROM "Operations"."BackgroundJob" WHERE "Id" = '{jobId}')""");
                throw new TimeoutException($"40 秒内没有做完。作业：{job}；文件：{file}；响应：{await response.Content.ReadAsStringAsync(Ct)}");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), TimeProvider.System, Ct);
        }
    }

    // 结束事件经发件箱、路由、收件箱交给 Notifications 后才生成站内消息
    private async Task<JsonElement> SiteMessageWhenSentAsync(string token, string title)
    {
        var deadline = TimeProvider.System.GetUtcNow().AddSeconds(40);
        while (true)
        {
            var list = await JsonAsync(await SendAsync(HttpMethod.Get, "/api/v1/notifications/site-messages?pageSize=100", token));
            var found = list.GetProperty("items").EnumerateArray().Where(i => string.Equals(i.GetProperty("title").GetString(), title, StringComparison.Ordinal)).ToList();
            if (found.Count > 0)
            {
                return found.ShouldHaveSingleItem();
            }

            if (TimeProvider.System.GetUtcNow() > deadline)
            {
                throw new TimeoutException($"40 秒内没有收到站内消息“{title}”。");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), TimeProvider.System, Ct);
        }
    }

    private static async Task<string[]> LinesOfAsync(HttpResponseMessage response)
    {
        var bytes = await response.Content.ReadAsByteArrayAsync(Ct);
        bytes[..3].ShouldBe([0xEF, 0xBB, 0xBF], "带 BOM 的 UTF-8，Excel 才会按 UTF-8 打开");
        return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
    }

    [Fact]
    public async Task TheSubmitter_GetsACsvOfExactlyTheFilteredRecords_WithNamesAndFormulaSafeCells()
    {
        var token = await LoginAsOperatorAsync("export-submitter", _export);
        var actor = await Seed.UserAsync("export-actor", Password, displayName: "导出里的操作人");
        var marker = $"Export{Guid.CreateVersion7():N}";
        await RecordAsync(Message(_now.AddMinutes(-30), marker, "Test.First", actor));
        await RecordAsync(Message(_now.AddMinutes(-20), marker, "=SUM(1,1)"));
        await RecordAsync(Message(_now.AddMinutes(-10), "OtherModule", "Test.NotThis"));

        var jobId = await SubmitAsync(token, Request(marker));
        var download = await DownloadWhenReadyAsync(token, jobId);

        download.StatusCode.ShouldBe(HttpStatusCode.OK);
        download.Content.Headers.ContentType!.MediaType.ShouldBe("text/csv");
        download.Content.Headers.ContentDisposition!.DispositionType.ShouldBe("attachment");
        download.Content.Headers.ContentDisposition.FileNameStar.ShouldStartWith("审计导出-");
        var lines = await LinesOfAsync(download);
        lines.Length.ShouldBe(3, "表头加两条，别的模块的不在里面");
        lines[0].ShouldStartWith("发生时间,模块,动作");
        lines[1].ShouldContain("Test.First");
        lines[1].ShouldContain("导出里的操作人");
        lines[2].ShouldContain("\"'=SUM(1,1)\"", Case.Sensitive, "以等号开头的单元格前面补单引号，不会被当成公式");

        var notice = await SiteMessageWhenSentAsync(token, "审计导出已完成");
        notice.GetProperty("body").GetString().ShouldBe("后台作业“审计导出”已完成。");
    }

    [Fact]
    public async Task AnotherExporter_CannotDownloadIt_ButWhoeverManagesJobsCan()
    {
        var submitter = await LoginAsOperatorAsync("export-owner", _export);
        var other = await LoginAsOperatorAsync("export-other", _export);
        var manager = await LoginAsOperatorAsync("export-manager", _exportAndManage);
        var plain = await LoginAsOperatorAsync("export-plain", []);
        var marker = $"Export{Guid.CreateVersion7():N}";
        await RecordAsync(Message(_now.AddMinutes(-5), marker, "Test.Only"));
        var jobId = await SubmitAsync(submitter, Request(marker));
        (await DownloadWhenReadyAsync(submitter, jobId)).StatusCode.ShouldBe(HttpStatusCode.OK);

        var otherResponse = await SendAsync(HttpMethod.Get, $"{Root}/{jobId}/file", other);
        var managerResponse = await SendAsync(HttpMethod.Get, $"{Root}/{jobId}/file", manager);
        var plainResponse = await SendAsync(HttpMethod.Get, $"{Root}/{jobId}/file", plain);
        var unknown = await SendAsync(HttpMethod.Get, $"{Root}/{Guid.CreateVersion7()}/file", submitter);

        otherResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await JsonAsync(otherResponse)).GetProperty("code").GetString().ShouldBe("Operations.AuditExportNotFound");
        managerResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        plainResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AFinishedExportWhoseFileIsNotRegisteredYet_IsRetryable_ButNotForever()
    {
        var manager = await LoginAsOperatorAsync("export-pending-manager", _exportAndManage);
        var justFinished = await InsertFinishedAsync(_now.AddSeconds(-5));
        var longAgo = await InsertFinishedAsync(_now.AddHours(-1));

        var pending = await SendAsync(HttpMethod.Get, $"{Root}/{justFinished}/file", manager);
        var missing = await SendAsync(HttpMethod.Get, $"{Root}/{longAgo}/file", manager);

        pending.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await JsonAsync(pending)).GetProperty("code").GetString().ShouldBe("Operations.AuditExportNotReady");
        missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await JsonAsync(missing)).GetProperty("code").GetString().ShouldBe("Files.NotFound");
    }

    // 已成功、带着结果文件 ID，但 Files 里还没有这个文件
    private async Task<Guid> InsertFinishedAsync(DateTimeOffset finished)
    {
        var job = Guid.CreateVersion7();
        await Seed.SqlAsync(
            string.Create(
                CultureInfo.InvariantCulture,
                $"""
                INSERT INTO "Operations"."BackgroundJob" ("Id", "JobType", "SchemaVersion", "Args", "SubmittedAt", "Status", "ProgressDone", "Attempts", "StartedAt", "FinishedAt", "ResultFileId")
                VALUES ('{job}', 'Operations.ExportAudit', 1, '{EmptyArgs}', '{finished.AddMinutes(-1):O}', 2, 0, 1, '{finished.AddMinutes(-1):O}', '{finished:O}', '{Guid.CreateVersion7()}')
                """));
        return job;
    }

    [Fact]
    public async Task Submitting_NeedsThePermission_AndAValidRange()
    {
        var plain = await LoginAsOperatorAsync("export-submit-plain", []);
        var exporter = await LoginAsOperatorAsync("export-submit-ok", _export);

        var forbidden = await SendAsync(HttpMethod.Post, Root, plain, Request("Any"));
        var backwards = await SendAsync(HttpMethod.Post, Root, exporter, new { from = _now, to = _now.AddDays(-1) });

        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        backwards.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MoreRowsThanTheLimit_FailTheJob_AndThereIsNothingToDownload()
    {
        var token = await LoginAsOperatorAsync("export-too-many", _export);
        var marker = $"Export{Guid.CreateVersion7():N}";
        for (var i = 0; i < 4; i++)
        {
            await RecordAsync(Message(_now.AddMinutes(-i - 1), marker, $"Test.Row{i}"));
        }

        var jobId = await SubmitAsync(token, Request(marker));
        var response = await DownloadWhenReadyAsync(token, jobId);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Operations.AuditExportFailed");

        var notice = await SiteMessageWhenSentAsync(token, "审计导出失败");
        notice.GetProperty("body").GetString()!.ShouldStartWith("后台作业“审计导出”失败。原因：");
    }
}
