using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Operations.Application.ClientErrors;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;

namespace Kmic.Api.Tests.Operations;

/// <summary>
/// 前端错误上报：登录就能报、白名单与长度上限、按用户限流、只写日志
/// </summary>
public sealed class ClientErrorTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(
        database,
        template,
        new Dictionary<string, string>(StringComparer.Ordinal) { ["Operations:ClientErrors:RequestsPerUserPerMinute"] = "4" }),
        IClassFixture<ClassDatabase<ApiTemplate>>,
        IDisposable
{
    private const string Url = "/api/v1/operations/client-errors";

    private readonly RecordingLog _log = new();
    private WebApplicationFactory<Program>? _recording;

    private static object Error(string message = "Cannot read properties of undefined", string type = "TypeError") =>
        new { type, message, stack = "at render (app.js:10:5)", routeName = "sample.specimens", appVersion = "1.4.2", traceId = "0123456789abcdef0123456789abcdef", occurredAt = "2026-11-13T02:00:00Z" };

    private HttpClient Client(string token)
    {
        _recording ??= Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IClientErrorLog>();
            services.AddSingleton<IClientErrorLog>(_log);
        }));
        var client = _recording.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private Task<HttpResponseMessage> ReportAsync(string token, object body) => Client(token).PostAsJsonAsync(Url, body, Ct);

    public void Dispose() => _recording?.Dispose();

    [Fact]
    public async Task ALoggedInUser_ReportsABatch_AndItIsLoggedFieldByField()
    {
        var token = await LoginAsOperatorAsync("client-error-reporter", []);

        var response = await ReportAsync(token, new { errors = new[] { Error(), Error("请求失败 500", "ApiError") } });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var reporter = _log.Written.Where(w => Said(w, "请求失败 500") || w.Report.Message.StartsWith("Cannot", StringComparison.Ordinal)).ToList();
        reporter.Count.ShouldBe(2);
        var first = reporter.Single(w => string.Equals(w.Report.Type, "TypeError", StringComparison.Ordinal)).Report;
        first.Stack.ShouldBe("at render (app.js:10:5)");
        first.RouteName.ShouldBe("sample.specimens");
        first.AppVersion.ShouldBe("1.4.2");
        first.TraceId.ShouldBe("0123456789abcdef0123456789abcdef");
        first.OccurredAt.ShouldBe(new DateTimeOffset(2026, 11, 13, 2, 0, 0, TimeSpan.Zero));
        reporter.Select(w => w.User).Distinct().Count().ShouldBe(1);
    }

    [Fact]
    public async Task AnAccountThatMustChangeItsPassword_CanStillReport()
    {
        // 出错的页面恰好可能是改密码页，这时更需要上报
        var role = await Seed.RoleAsync("无权限点的角色", []);
        var user = await Seed.UserAsync("client-error-must-change", Password, mustChange: true);
        await Seed.GrantAsync(user, role);
        var token = (await LoginTokensAsync("client-error-must-change")).GetProperty("access_token").GetString()!;

        var response = await ReportAsync(token, new { errors = new[] { Error("改密码页出错") } });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        _log.Written.ShouldContain(w => Said(w, "改密码页出错"));
    }

    [Fact]
    public async Task ABatchThatBreaksTheWhitelistOrTheLimits_IsRefusedWholly()
    {
        var token = await LoginAsOperatorAsync("client-error-strict", []);
        var before = _log.Written.Count;

        var empty = await ReportAsync(token, new { errors = Array.Empty<object>() });
        var tooMany = await ReportAsync(token, new { errors = Enumerable.Range(0, 21).Select(i => Error($"第 {i} 条")).ToArray() });
        var tooLong = await ReportAsync(token, new { errors = new[] { Error(new string('长', 1001)) } });
        var noType = await ReportAsync(token, new { errors = new[] { Error(type: "") } });
        var badTrace = await ReportAsync(token, new { errors = new[] { new { type = "TypeError", message = "x", traceId = "不是追踪 ID" } } });
        var unknownField = await ReportAsync(token, new { errors = new[] { new { type = "TypeError", message = "x", formValues = "password=1" } } });

        foreach (var refused in new[] { empty, tooMany, tooLong, noType, badTrace, unknownField })
        {
            refused.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        (await JsonAsync(tooMany)).GetProperty("errors")[0].GetProperty("path").GetString().ShouldBe("errors");
        (await JsonAsync(tooLong)).GetProperty("errors")[0].GetProperty("path").GetString().ShouldBe("errors[0].message");
        _log.Written.Count.ShouldBe(before, "被拒绝的请求一条也不写");
    }

    [Fact]
    public async Task APartlyBadBatch_WritesNothing()
    {
        var token = await LoginAsOperatorAsync("client-error-partial", []);

        var response = await ReportAsync(token, new { errors = new[] { Error("好的那条"), Error(new string('长', 1001)) } });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        _log.Written.ShouldNotContain(w => Said(w, "好的那条"));
    }

    [Fact]
    public async Task TooManyRequestsFromOneUser_Get429WithRetryAfter_WhileAnotherUserIsUnaffected()
    {
        var noisy = await LoginAsOperatorAsync("client-error-noisy", []);
        var quiet = await LoginAsOperatorAsync("client-error-quiet", []);

        var statuses = new List<HttpStatusCode>();
        HttpResponseMessage? limited = null;
        for (var i = 0; i < 5; i++)
        {
            var response = await ReportAsync(noisy, new { errors = new[] { Error($"刷屏 {i}") } });
            statuses.Add(response.StatusCode);
            limited = response;
        }

        statuses.Take(4).ShouldAllBe(s => s == HttpStatusCode.NoContent);
        statuses[4].ShouldBe(HttpStatusCode.TooManyRequests);
        limited!.Headers.RetryAfter.ShouldNotBeNull();
        (await JsonAsync(limited)).GetProperty("code").GetString().ShouldBe("Operations.ClientErrorsThrottled");
        _log.Written.ShouldNotContain(w => Said(w, "刷屏 4"));
        (await ReportAsync(quiet, new { errors = new[] { Error("别人的错误") } })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task WithoutLoggingIn_ItIsRefused()
    {
        using var client = NewClient();

        var response = await client.PostAsJsonAsync(Url, new { errors = new[] { Error() } }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static bool Said((Guid User, ClientErrorReport Report) written, string message) =>
        string.Equals(written.Report.Message, message, StringComparison.Ordinal);

    private sealed class RecordingLog : IClientErrorLog
    {
        public ConcurrentQueue<(Guid User, ClientErrorReport Report)> Written { get; } = new();

        public void Write(UserId user, ClientErrorReport report) => Written.Enqueue((user.Value, report));
    }
}
