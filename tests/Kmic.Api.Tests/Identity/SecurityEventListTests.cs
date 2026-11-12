using System.Net;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Identity.Application;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 安全事件列表：时间范围必填、按类型账号筛选、权限，端到端
/// </summary>
public sealed class SecurityEventListTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private static string Range(TimeSpan before, TimeSpan after) =>
        $"from={Uri.EscapeDataString(TimeProvider.System.GetUtcNow().Add(-before).ToString("o"))}&to={Uri.EscapeDataString(TimeProvider.System.GetUtcNow().Add(after).ToString("o"))}";

    private static string Hour => Range(TimeSpan.FromHours(1), TimeSpan.FromHours(1));

    [Fact]
    public async Task FailedLogins_AreListedWithTheAttemptedAccount_AndCanBeFiltered()
    {
        var admin = await LoginAsOperatorAsync("events-viewer", [IdentityPermissions.SecurityEventView]);
        for (var i = 0; i < 2; i++)
        {
            await PostAsync(NewClient(), "/api/v1/identity/auth/login", new { account = "event.ghost", password = "wrong password here", clientType = "app" });
        }

        await WaitForSecurityEventsAsync("\"Type\" = 'LoginFailed' AND \"AttemptedAccount\" = 'event.ghost'", atLeast: 2);
        var response = await SendAsync(HttpMethod.Get, $"/security-events?{Hour}&type=LoginFailed&account=event.ghost", admin);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await JsonAsync(response);
        body.GetProperty("totalCount").GetInt64().ShouldBe(2);
        var first = body.GetProperty("items")[0];
        first.GetProperty("type").GetString().ShouldBe("LoginFailed");
        first.GetProperty("severity").GetString().ShouldBe("Warning");
        first.GetProperty("reason").GetString().ShouldBe("UnknownAccount");
        first.GetProperty("attemptedAccount").GetString().ShouldBe("event.ghost");
        first.GetProperty("details").GetProperty("client").GetString().ShouldBe("app");
        first.GetProperty("traceId").GetString().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task TheTimeRange_IsRequired_AndHasAnUpperLimit()
    {
        var admin = await LoginAsOperatorAsync("events-range", [IdentityPermissions.SecurityEventView]);

        var missing = await SendAsync(HttpMethod.Get, "/security-events", admin);
        var backwards = await SendAsync(HttpMethod.Get, $"/security-events?{Range(TimeSpan.Zero, TimeSpan.FromHours(-1))}", admin);
        var tooLong = await SendAsync(HttpMethod.Get, $"/security-events?{Range(TimeSpan.FromDays(40), TimeSpan.Zero)}", admin);
        var exactly = await SendAsync(HttpMethod.Get, $"/security-events?{Range(TimeSpan.FromDays(30), TimeSpan.Zero)}", admin);

        missing.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        backwards.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        tooLong.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exactly.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OnlyThoseWithThePermission_CanRead()
    {
        var nobody = await LoginAsOperatorAsync("events-nobody", [IdentityPermissions.UserView]);

        var response = await SendAsync(HttpMethod.Get, $"/security-events?{Hour}", nobody);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task APageOfEvents_CanBeSortedAndCut()
    {
        var admin = await LoginAsOperatorAsync("events-page", [IdentityPermissions.SecurityEventView]);
        for (var i = 0; i < 3; i++)
        {
            await PostAsync(NewClient(), "/api/v1/identity/auth/login", new { account = "event.pager", password = "wrong password here", clientType = "app" });
        }

        await WaitForSecurityEventsAsync("\"Type\" = 'LoginFailed' AND \"AttemptedAccount\" = 'event.pager'", atLeast: 3);
        var newest = await JsonAsync(await SendAsync(HttpMethod.Get, $"/security-events?{Hour}&account=event.pager&pageSize=2", admin));
        var oldest = await JsonAsync(await SendAsync(HttpMethod.Get, $"/security-events?{Hour}&account=event.pager&pageSize=2&sort=occurredAt", admin));
        var second = await JsonAsync(await SendAsync(HttpMethod.Get, $"/security-events?{Hour}&account=event.pager&pageSize=2&page=2", admin));

        newest.GetProperty("items").GetArrayLength().ShouldBe(2);
        newest.GetProperty("totalCount").GetInt64().ShouldBe(3);
        second.GetProperty("items").GetArrayLength().ShouldBe(1);
        var newestTimes = newest.GetProperty("items").EnumerateArray().Select(e => e.GetProperty("occurredAt").GetDateTimeOffset()).ToList();
        var oldestTimes = oldest.GetProperty("items").EnumerateArray().Select(e => e.GetProperty("occurredAt").GetDateTimeOffset()).ToList();
        newestTimes[0].ShouldBeGreaterThanOrEqualTo(newestTimes[1]);
        oldestTimes[0].ShouldBeLessThanOrEqualTo(oldestTimes[1]);
    }
}
