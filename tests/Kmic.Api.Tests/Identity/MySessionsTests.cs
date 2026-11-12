using System.Net;
using System.Text.Json;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Sample.Application;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 我的登录设备：自己查看，结束别的设备，不能碰别人的会话，端到端
/// </summary>
public sealed class MySessionsTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private async Task<string> UserAsync(string account)
    {
        var role = await Seed.RoleAsync("设备-" + account, [SamplePermissions.View]);
        await Seed.GrantAsync(await Seed.UserAsync(account, Password, displayName: "设备测试 " + account), role);
        return account;
    }

    private async Task<JsonElement> SessionsAsync(string token) => await JsonAsync(await SendAsync(HttpMethod.Get, "/me/sessions", token));

    [Fact]
    public async Task TheList_ShowsEveryActiveSession_AndMarksTheCurrentOne()
    {
        await UserAsync("device.alice");
        var phone = (await LoginTokensAsync("device.alice")).GetProperty("access_token").GetString()!;
        var laptop = (await LoginTokensAsync("device.alice")).GetProperty("access_token").GetString()!;

        var sessions = await SessionsAsync(phone);

        sessions.GetArrayLength().ShouldBe(2);
        sessions.EnumerateArray().Count(s => s.GetProperty("isCurrent").GetBoolean()).ShouldBe(1);
        sessions.EnumerateArray().All(s => string.Equals(s.GetProperty("clientType").GetString(), "App", StringComparison.Ordinal)).ShouldBeTrue();
        (await SessionsAsync(laptop)).EnumerateArray().Count(s => s.GetProperty("isCurrent").GetBoolean()).ShouldBe(1);
    }

    [Fact]
    public async Task EndingAnotherDevice_LogsItOut_AndLeavesTheCurrentOneAlone()
    {
        await UserAsync("device.bob");
        var phone = (await LoginTokensAsync("device.bob")).GetProperty("access_token").GetString()!;
        var laptop = (await LoginTokensAsync("device.bob")).GetProperty("access_token").GetString()!;
        var other = (await SessionsAsync(phone)).EnumerateArray().Single(s => !s.GetProperty("isCurrent").GetBoolean()).GetProperty("id").GetGuid();

        var ended = await SendAsync(HttpMethod.Delete, $"/me/sessions/{other}", phone);
        var again = await SendAsync(HttpMethod.Delete, $"/me/sessions/{other}", phone);

        ended.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        again.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await GetAsync(NewClient(), "/api/v1/identity/me", phone)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetAsync(NewClient(), "/api/v1/identity/me", laptop)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await SessionsAsync(phone)).GetArrayLength().ShouldBe(1);
    }

    [Fact]
    public async Task SomeoneElsesSession_LooksLikeASessionThatDoesNotExist()
    {
        await UserAsync("device.carol");
        await UserAsync("device.dave");
        var carol = (await LoginTokensAsync("device.carol")).GetProperty("access_token").GetString()!;
        var dave = (await LoginTokensAsync("device.dave")).GetProperty("access_token").GetString()!;
        var dsSession = (await SessionsAsync(dave))[0].GetProperty("id").GetGuid();

        var foreign = await SendAsync(HttpMethod.Delete, $"/me/sessions/{dsSession}", carol);
        var missing = await SendAsync(HttpMethod.Delete, $"/me/sessions/{Guid.CreateVersion7()}", carol);

        foreign.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetAsync(NewClient(), "/api/v1/identity/me", dave)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
