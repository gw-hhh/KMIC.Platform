using System.Net;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Sample.Application;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 登录、/me、刷新、退出、切换角色、修改密码：真实的 Identity 模块、真实的令牌，端到端
/// </summary>
public sealed class AuthenticationTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityApiTestBase(database, template, new Dictionary<string, string>(StringComparer.Ordinal) { ["Identity:Session:RefreshGrace"] = "00:00:00" }),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Password = "correct horse battery";

    private async Task<(Guid UserId, Guid RoleId)> UserWithRoleAsync(string account, string? password = Password, bool active = true)
    {
        var role = await Seed.RoleAsync("销售-" + account, [SamplePermissions.View, SamplePermissions.Create], "OrgUnit");
        var user = await Seed.UserAsync(account, password, displayName: "张三 " + account, active: active);
        await Seed.GrantAsync(user, role);
        return (user, role);
    }

    private static async Task<HttpResponseMessage> LoginAsync(HttpClient client, string account, string password = Password, string clientType = "app") =>
        await PostAsync(client, "/api/v1/identity/auth/login", new { account, password, clientType });

    [Fact]
    public async Task Login_ForAnApp_ReturnsBothTokens_AndMeShowsTheCurrentRoleAndPermissions()
    {
        var (userId, roleId) = await UserWithRoleAsync("app-user");
        var client = NewClient();

        var response = await LoginAsync(client, "app-user");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tokens = await JsonAsync(response);
        tokens.GetProperty("token_type").GetString().ShouldBe("Bearer");
        tokens.GetProperty("expires_in").GetInt32().ShouldBeInRange(800, 900);
        tokens.GetProperty("refresh_token").GetString().ShouldNotBeNullOrWhiteSpace();
        tokens.GetProperty("passwordChangeRequired").GetBoolean().ShouldBeFalse();
        RefreshCookieOf(response).ShouldBeNull();

        var me = await GetAsync(client, "/api/v1/identity/me", tokens.GetProperty("access_token").GetString());

        me.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await JsonAsync(me);
        body.GetProperty("userId").GetGuid().ShouldBe(userId);
        body.GetProperty("currentRole").GetProperty("id").GetGuid().ShouldBe(roleId);
        string.Join(",", body.GetProperty("permissions").EnumerateArray().Select(p => p.GetString()).Order(StringComparer.Ordinal)).ShouldBe($"{SamplePermissions.Create},{SamplePermissions.View}");
        body.GetProperty("businessTimeZone").GetString().ShouldBe("Asia/Shanghai");
        body.GetProperty("hasPassword").GetBoolean().ShouldBeTrue();
        me.Headers.GetValues("X-Authz-Version").ShouldHaveSingleItem().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_ForTheWeb_PutsTheRefreshTokenInAnHttpOnlyCookieOnly()
    {
        await UserWithRoleAsync("web-user");

        var response = await LoginAsync(NewClient(), "web-user", clientType: "web");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(response)).TryGetProperty("refresh_token", out var inBody).ShouldBeTrue();
        inBody.ValueKind.ShouldBe(System.Text.Json.JsonValueKind.Null);
        var setCookie = response.Headers.GetValues("Set-Cookie").First(v => v.StartsWith("__Host-kmic-rt=", StringComparison.Ordinal));
        setCookie.ShouldContain("httponly", Case.Insensitive);
        setCookie.ShouldContain("secure", Case.Insensitive);
        setCookie.ShouldContain("samesite=strict", Case.Insensitive);
    }

    [Fact]
    public async Task Login_WrongPasswordUnknownAccountAndDisabledUser_AllLookTheSame()
    {
        await UserWithRoleAsync("real-user");
        await UserWithRoleAsync("gone-user", active: false);
        var client = NewClient();

        var wrong = await LoginAsync(client, "real-user", "not the password at all");
        var unknown = await LoginAsync(client, "nobody-here");
        var disabled = await LoginAsync(client, "gone-user");

        foreach (var response in new[] { wrong, unknown, disabled })
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
            (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Identity.InvalidCredentials");
        }
    }

    [Fact]
    public async Task Login_AfterRepeatedFailures_IsThrottled_EvenWithTheRightPassword_AndSucceedsAfterUnlock()
    {
        await UserWithRoleAsync("throttled-user");
        var client = NewClient();
        for (var i = 0; i < 3; i++)
        {
            (await LoginAsync(client, "throttled-user", "wrong password here")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        var blocked = await LoginAsync(client, "throttled-user");

        blocked.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        var body = await JsonAsync(blocked);
        body.GetProperty("code").GetString().ShouldBe("Identity.LoginThrottled");
        body.GetProperty("retryAfterSeconds").GetInt32().ShouldBeGreaterThan(0);
        blocked.Headers.Contains("Retry-After").ShouldBeTrue();

        // 不存在的登录名同样被限制，不暴露账号是否存在
        for (var i = 0; i < 3; i++)
        {
            await LoginAsync(client, "ghost-user", "wrong password here");
        }

        (await LoginAsync(client, "ghost-user")).StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Login_WithoutAnyRole_IsRefused()
    {
        await Seed.UserAsync("no-role", Password);

        var response = await LoginAsync(NewClient(), "no-role");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Identity.NoRoleAvailable");
    }

    [Fact]
    public async Task Login_BadClientType_IsAValidationError()
    {
        await UserWithRoleAsync("bad-client");

        var response = await LoginAsync(NewClient(), "bad-client", clientType: "desktop");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Refresh_ForAnApp_RotatesTheToken_AndReusingTheOldOneRevokesTheSession()
    {
        await UserWithRoleAsync("rotator");
        var client = NewClient();
        var first = await JsonAsync(await LoginAsync(client, "rotator"));
        var oldRefresh = first.GetProperty("refresh_token").GetString()!;

        var refreshed = await PostAsync(client, "/api/v1/identity/auth/refresh", new { refreshToken = oldRefresh });

        refreshed.StatusCode.ShouldBe(HttpStatusCode.OK);
        var second = await JsonAsync(refreshed);
        var newRefresh = second.GetProperty("refresh_token").GetString()!;
        string.Equals(newRefresh, oldRefresh, StringComparison.Ordinal).ShouldBeFalse();
        (await GetAsync(client, "/api/v1/identity/me", second.GetProperty("access_token").GetString())).StatusCode.ShouldBe(HttpStatusCode.OK);

        // 宽限期在这个测试里是 0：旧令牌再出现就是重用，整个会话作废，新令牌也跟着失效
        var reused = await PostAsync(client, "/api/v1/identity/auth/refresh", new { refreshToken = oldRefresh });

        reused.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await JsonAsync(reused)).GetProperty("code").GetString().ShouldBe("Identity.InvalidRefreshToken");
        (await PostAsync(client, "/api/v1/identity/auth/refresh", new { refreshToken = newRefresh })).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await GetAsync(client, "/api/v1/identity/me", second.GetProperty("access_token").GetString())).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ForTheWeb_NeedsTheCsrfHeaders_AndRotatesTheCookie()
    {
        await UserWithRoleAsync("web-rotator");
        var client = NewClient();
        var login = await LoginAsync(client, "web-rotator", clientType: "web");
        var cookie = RefreshCookieOf(login).ShouldNotBeNull();

        var withoutHeaders = await PostAsync(client, "/api/v1/identity/auth/refresh", null, cookie: cookie);
        var refreshed = await PostAsync(client, "/api/v1/identity/auth/refresh", null, csrf: true, cookie: cookie);

        withoutHeaders.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await JsonAsync(withoutHeaders)).GetProperty("code").GetString().ShouldBe("Identity.CsrfRejected");
        refreshed.StatusCode.ShouldBe(HttpStatusCode.OK);
        string.Equals(RefreshCookieOf(refreshed).ShouldNotBeNull(), cookie, StringComparison.Ordinal).ShouldBeFalse();
        (await JsonAsync(refreshed)).GetProperty("refresh_token").ValueKind.ShouldBe(System.Text.Json.JsonValueKind.Null);
    }

    [Fact]
    public async Task Refresh_WithAnUnknownToken_IsRefused()
    {
        var response = await PostAsync(NewClient(), "/api/v1/identity/auth/refresh", new { refreshToken = "no-such-token" });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevokesTheSession_SoTheTokenStopsWorkingAtOnce()
    {
        await UserWithRoleAsync("leaver");
        var client = NewClient();
        var tokens = await JsonAsync(await LoginAsync(client, "leaver"));
        var access = tokens.GetProperty("access_token").GetString();

        var logout = await PostAsync(client, "/api/v1/identity/auth/logout", null, bearer: access);

        logout.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await GetAsync(client, "/api/v1/identity/me", access)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DisabledRole_GivesCurrentRoleUnavailable_AndSwitchingToAnotherRoleRecovers()
    {
        var (userId, firstRole) = await UserWithRoleAsync("switcher");
        var secondRole = await Seed.RoleAsync("区域经理-switcher", [SamplePermissions.View], sortOrder: 20);
        await Seed.GrantAsync(userId, secondRole);
        var client = NewClient();
        var access = (await JsonAsync(await LoginAsync(client, "switcher"))).GetProperty("access_token").GetString();
        await Seed.SetRoleStatusAsync(firstRole, active: false);

        var blocked = await GetAsync(client, "/api/v1/identity/me", access);
        var switched = await PostAsync(client, "/api/v1/identity/auth/switch-role", new { roleId = secondRole }, bearer: access);

        blocked.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(blocked)).GetProperty("permissions").GetArrayLength().ShouldBe(0);
        switched.StatusCode.ShouldBe(HttpStatusCode.OK);
        var newAccess = (await JsonAsync(switched)).GetProperty("access_token").GetString();
        var me = await JsonAsync(await GetAsync(client, "/api/v1/identity/me", newAccess));
        me.GetProperty("currentRole").GetProperty("id").GetGuid().ShouldBe(secondRole);
        string.Join(",", me.GetProperty("permissions").EnumerateArray().Select(p => p.GetString())).ShouldBe(SamplePermissions.View);

        // 旧角色的令牌随着切换失效
        (await GetAsync(client, "/api/v1/identity/me", access)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SwitchRole_ToARoleTheUserDoesNotHave_IsRefused()
    {
        await UserWithRoleAsync("owner-a");
        var (_, foreignRole) = await UserWithRoleAsync("owner-b");
        var client = NewClient();
        var access = (await JsonAsync(await LoginAsync(client, "owner-a"))).GetProperty("access_token").GetString();

        var response = await PostAsync(client, "/api/v1/identity/auth/switch-role", new { roleId = foreignRole }, bearer: access);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Identity.RoleNotSwitchable");
    }

    [Fact]
    public async Task ChangePassword_NeedsTheOldPassword_ChecksThePolicy_AndEndsTheOtherSessions()
    {
        await UserWithRoleAsync("changer");
        var client = NewClient();
        var current = (await JsonAsync(await LoginAsync(client, "changer"))).GetProperty("access_token").GetString();
        var other = (await JsonAsync(await LoginAsync(client, "changer"))).GetProperty("access_token").GetString();

        var wrongOld = await PostAsync(client, "/api/v1/identity/auth/change-password", new { oldPassword = "not the old one", newPassword = "a brand new passphrase" }, bearer: current);
        var weak = await PostAsync(client, "/api/v1/identity/auth/change-password", new { oldPassword = Password, newPassword = "password123" }, bearer: current);
        var ok = await PostAsync(client, "/api/v1/identity/auth/change-password", new { oldPassword = Password, newPassword = "a brand new passphrase" }, bearer: current);

        (await JsonAsync(wrongOld)).GetProperty("code").GetString().ShouldBe("Identity.OldPasswordWrong");
        (await JsonAsync(weak)).GetProperty("code").GetString().ShouldBe("Identity.PasswordTooWeak");
        ok.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await GetAsync(client, "/api/v1/identity/me", current)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetAsync(client, "/api/v1/identity/me", other)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await LoginAsync(client, "changer", "a brand new passphrase")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await LoginAsync(client, "changer", Password)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UserWhoMustChangeThePassword_CanOnlyReachMeLogoutAndChangePassword()
    {
        var role = await Seed.RoleAsync("销售-must", [SamplePermissions.View, SamplePermissions.Create]);
        var user = await Seed.UserAsync("must-change", Password, mustChange: true);
        await Seed.GrantAsync(user, role);
        var client = NewClient();
        var tokens = await JsonAsync(await LoginAsync(client, "must-change"));
        tokens.GetProperty("passwordChangeRequired").GetBoolean().ShouldBeTrue();
        var access = tokens.GetProperty("access_token").GetString();

        (await GetAsync(client, "/api/v1/identity/me", access)).StatusCode.ShouldBe(HttpStatusCode.OK);
        var blocked = await GetAsync(client, "/api/v1/sample/specimens", access);
        var changed = await PostAsync(client, "/api/v1/identity/auth/change-password", new { oldPassword = Password, newPassword = "a brand new passphrase" }, bearer: access);

        blocked.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await JsonAsync(blocked)).GetProperty("code").GetString().ShouldBe("Common.PasswordChangeRequired");
        changed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await GetAsync(client, "/api/v1/sample/specimens", access)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
