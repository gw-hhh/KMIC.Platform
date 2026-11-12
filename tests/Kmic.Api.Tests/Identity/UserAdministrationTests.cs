using System.Net;
using System.Text.Json;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Identity.Application;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 用户管理：新建、改资料、禁用启用、重置密码、解锁、强制下线，以及列表、详情、选人下拉，端到端
/// </summary>
public sealed class UserAdministrationTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private static readonly string[] _all =
    [
        IdentityPermissions.UserView,
        IdentityPermissions.UserManage,
        IdentityPermissions.UserResetPassword,
        IdentityPermissions.UserUnlock,
        IdentityPermissions.UserForceLogout,
    ];

    private async Task<Guid> CreateUserAsync(string token, string account, string displayName = "新同事", string? mobile = null, string? employeeNo = null)
    {
        var response = await SendAsync(HttpMethod.Post, "/users", token, new { account, displayName, englishName = (string?)null, employeeNo, mobile, email = (string?)null });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await JsonAsync(response)).GetProperty("id").GetGuid();
    }

    private async Task<JsonElement> DetailAsync(string token, Guid id)
    {
        var response = await SendAsync(HttpMethod.Get, $"/users/{id}", token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await JsonAsync(response);
    }

    private async Task<long> VersionAsync(string token, Guid id) => (await DetailAsync(token, id)).GetProperty("version").GetInt64();

    private static async Task<string> CodeOfAsync(HttpResponseMessage response) => (await JsonAsync(response)).GetProperty("code").GetString()!;

    // 造一个带密码、拥有普通角色的用户，用来被管理
    private async Task<Guid> SeedMemberAsync(string account, string? mobile = null, bool protectedRole = false)
    {
        var role = await Seed.RoleAsync("成员-" + account, [IdentityPermissions.RoleView], isProtected: protectedRole);
        var user = await Seed.UserAsync(account, Password, displayName: "成员 " + account, mobile: mobile is null ? null : "+86" + mobile);
        await Seed.GrantAsync(user, role);
        return user;
    }

    [Fact]
    public async Task CreateUser_ReturnsItsLocation_AndTheDetailShowsNoPasswordYet()
    {
        var token = await LoginAsOperatorAsync("create-admin", _all);

        var response = await SendAsync(HttpMethod.Post, "/users", token, new { account = "New.Person", displayName = "新同事", englishName = "Newbie", employeeNo = "E-1001", mobile = "13800138001", email = "new@example.com" });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var id = (await JsonAsync(response)).GetProperty("id").GetGuid();
        response.Headers.Location!.ToString().ShouldEndWith($"/api/v1/identity/users/{id}");
        var detail = await DetailAsync(token, id);
        detail.GetProperty("account").GetString().ShouldBe("New.Person");
        detail.GetProperty("englishName").GetString().ShouldBe("Newbie");
        detail.GetProperty("employeeNo").GetString().ShouldBe("E-1001");
        detail.GetProperty("mobile").GetString().ShouldBe("+8613800138001");
        detail.GetProperty("status").GetString().ShouldBe("Active");
        detail.GetProperty("hasPassword").GetBoolean().ShouldBeFalse();
        detail.GetProperty("grants").GetArrayLength().ShouldBe(0);
        detail.GetProperty("loginLockedUntil").ValueKind.ShouldBe(JsonValueKind.Null);
        ActionsOf(detail).ShouldBe(["ChangeCredentials", "Disable", "Edit", "ForceLogout", "ResetPassword", "Unlock"]);
    }

    [Fact]
    public async Task CreateUser_WithADuplicateAccountMobileOrEmployeeNo_ReturnsTheMatchingConflict()
    {
        var token = await LoginAsOperatorAsync("dup-admin", _all);
        await CreateUserAsync(token, "dup.first", mobile: "13800138002", employeeNo: "E-2001");

        var account = await SendAsync(HttpMethod.Post, "/users", token, new { account = "DUP.FIRST", displayName = "重名", englishName = (string?)null, employeeNo = (string?)null, mobile = (string?)null, email = (string?)null });
        var mobile = await SendAsync(HttpMethod.Post, "/users", token, new { account = "dup.second", displayName = "重手机", englishName = (string?)null, employeeNo = (string?)null, mobile = "+86 138 0013 8002", email = (string?)null });
        var employeeNo = await SendAsync(HttpMethod.Post, "/users", token, new { account = "dup.third", displayName = "重工号", englishName = (string?)null, employeeNo = "E-2001", mobile = (string?)null, email = (string?)null });

        account.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(account)).ShouldBe("Identity.AccountExists");
        (await CodeOfAsync(mobile)).ShouldBe("Identity.MobileExists");
        (await CodeOfAsync(employeeNo)).ShouldBe("Identity.EmployeeNoExists");
    }

    [Fact]
    public async Task CreateUser_WithAMalformedMobileOrEmptyName_IsRejected()
    {
        var token = await LoginAsOperatorAsync("bad-admin", _all);

        var mobile = await SendAsync(HttpMethod.Post, "/users", token, new { account = "bad.mobile", displayName = "手机不对", englishName = (string?)null, employeeNo = (string?)null, mobile = "12345", email = (string?)null });
        var name = await SendAsync(HttpMethod.Post, "/users", token, new { account = "bad.name", displayName = " ", englishName = (string?)null, employeeNo = (string?)null, mobile = (string?)null, email = (string?)null });

        mobile.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await CodeOfAsync(mobile)).ShouldBe("Identity.MobileInvalid");
        name.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Viewing_NeedsOnlyTheViewPermission_AndEveryChangeNeedsItsOwn()
    {
        var viewer = await LoginAsOperatorAsync("view-only", [IdentityPermissions.UserView]);
        var manager = await LoginAsOperatorAsync("view-target-admin", _all);
        var id = await CreateUserAsync(manager, "view.target");
        var version = await VersionAsync(manager, id);

        var list = await SendAsync(HttpMethod.Get, "/users?q=view.target", viewer);
        var detail = await SendAsync(HttpMethod.Get, $"/users/{id}", viewer);
        var create = await SendAsync(HttpMethod.Post, "/users", viewer, new { account = "view.nope", displayName = "不该成功", englishName = (string?)null, employeeNo = (string?)null, mobile = (string?)null, email = (string?)null });
        var disable = await SendAsync(HttpMethod.Post, $"/users/{id}/disable", viewer, new { expectedVersion = version, reason = (string?)null });
        var reset = await SendAsync(HttpMethod.Post, $"/users/{id}/reset-password", viewer);
        var unlock = await SendAsync(HttpMethod.Post, $"/users/{id}/unlock-login", viewer);
        var logout = await SendAsync(HttpMethod.Post, $"/users/{id}/force-logout", viewer, new { sessionId = (Guid?)null });

        list.StatusCode.ShouldBe(HttpStatusCode.OK);
        detail.StatusCode.ShouldBe(HttpStatusCode.OK);
        ActionsOf(await JsonAsync(detail)).ShouldBeEmpty();
        foreach (var denied in new[] { create, disable, reset, unlock, logout })
        {
            denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }
    }

    [Fact]
    public async Task UpdateProfile_ChangesTheFields_AndAStaleVersionIsAConflict()
    {
        var token = await LoginAsOperatorAsync("edit-admin", _all);
        var id = await CreateUserAsync(token, "edit.target", "改名前");
        var version = await VersionAsync(token, id);

        var first = await SendAsync(HttpMethod.Put, $"/users/{id}", token, new { expectedVersion = version, account = "edit.target", displayName = "改名后", englishName = "Renamed", employeeNo = "E-3001", mobile = "13800138003", email = "renamed@example.com" });
        var stale = await SendAsync(HttpMethod.Put, $"/users/{id}", token, new { expectedVersion = version, account = "edit.target", displayName = "又改一次", englishName = (string?)null, employeeNo = (string?)null, mobile = (string?)null, email = (string?)null });

        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        var detail = await DetailAsync(token, id);
        detail.GetProperty("displayName").GetString().ShouldBe("改名后");
        detail.GetProperty("employeeNo").GetString().ShouldBe("E-3001");
        detail.GetProperty("mobile").GetString().ShouldBe("+8613800138003");
        detail.GetProperty("version").GetInt64().ShouldBeGreaterThan(version);
        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(stale)).ShouldBe("Common.VersionConflict");
    }

    [Fact]
    public async Task UpdateProfile_OfAProtectedHolder_NeedsAProtectedRoleOnlyForAccountAndMobile()
    {
        var ordinary = await LoginAsOperatorAsync("cred-ordinary", _all);
        var guardian = await LoginAsOperatorAsync("cred-guardian", _all, isProtected: true);
        var target = await SeedMemberAsync("cred.target", mobile: "13800138004", protectedRole: true);
        var version = await VersionAsync(guardian, target);

        var renamed = await SendAsync(HttpMethod.Put, $"/users/{target}", ordinary, new { expectedVersion = version, account = "cred.target", displayName = "只改姓名", englishName = (string?)null, employeeNo = (string?)null, mobile = "13800138004", email = (string?)null });
        version = await VersionAsync(guardian, target);
        var newMobile = await SendAsync(HttpMethod.Put, $"/users/{target}", ordinary, new { expectedVersion = version, account = "cred.target", displayName = "只改姓名", englishName = (string?)null, employeeNo = (string?)null, mobile = "13800138005", email = (string?)null });
        var newAccount = await SendAsync(HttpMethod.Put, $"/users/{target}", ordinary, new { expectedVersion = version, account = "cred.renamed", displayName = "只改姓名", englishName = (string?)null, employeeNo = (string?)null, mobile = "13800138004", email = (string?)null });
        var byGuardian = await SendAsync(HttpMethod.Put, $"/users/{target}", guardian, new { expectedVersion = version, account = "cred.target", displayName = "只改姓名", englishName = (string?)null, employeeNo = (string?)null, mobile = "13800138005", email = (string?)null });

        renamed.StatusCode.ShouldBe(HttpStatusCode.OK);
        newMobile.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CodeOfAsync(newMobile)).ShouldBe("Identity.ProtectedRoleChangeDenied");
        newAccount.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        byGuardian.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await DetailAsync(guardian, target)).GetProperty("mobile").GetString().ShouldBe("+8613800138005");
    }

    [Fact]
    public async Task Disable_RevokesTheirSessions_AndEnableLetsThemLogInAgain()
    {
        var token = await LoginAsOperatorAsync("toggle-admin", _all);
        var target = await SeedMemberAsync("toggle.target");
        var tokens = await LoginTokensAsync("toggle.target");
        var theirs = tokens.GetProperty("access_token").GetString()!;
        (await GetAsync(NewClient(), "/api/v1/identity/me", theirs)).StatusCode.ShouldBe(HttpStatusCode.OK);
        var version = await VersionAsync(token, target);

        var disabled = await SendAsync(HttpMethod.Post, $"/users/{target}/disable", token, new { expectedVersion = version, reason = "离职" });

        disabled.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetAsync(NewClient(), "/api/v1/identity/me", theirs)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await PostAsync(NewClient(), "/api/v1/identity/auth/login", new { account = "toggle.target", password = Password, clientType = "app" })).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var detail = await DetailAsync(token, target);
        detail.GetProperty("status").GetString().ShouldBe("Disabled");
        detail.GetProperty("activeSessionCount").GetInt32().ShouldBe(0);
        ActionsOf(detail).Contains("Disable", StringComparer.Ordinal).ShouldBeFalse();

        var enabled = await SendAsync(HttpMethod.Post, $"/users/{target}/enable", token, new { expectedVersion = detail.GetProperty("version").GetInt64() });

        enabled.StatusCode.ShouldBe(HttpStatusCode.OK);
        await LoginTokensAsync("toggle.target");
    }

    [Fact]
    public async Task Disable_Twice_IsAnInvalidStateAndNotAnError500()
    {
        var token = await LoginAsOperatorAsync("twice-admin", _all);
        var target = await SeedMemberAsync("twice.target");
        var version = await VersionAsync(token, target);
        (await SendAsync(HttpMethod.Post, $"/users/{target}/disable", token, new { expectedVersion = version, reason = (string?)null })).StatusCode.ShouldBe(HttpStatusCode.OK);
        version = await VersionAsync(token, target);

        var again = await SendAsync(HttpMethod.Post, $"/users/{target}/disable", token, new { expectedVersion = version, reason = (string?)null });
        var enableActive = await SendAsync(HttpMethod.Post, $"/users/{target}/enable", token, new { expectedVersion = version });
        version = await VersionAsync(token, target);
        var enableAgain = await SendAsync(HttpMethod.Post, $"/users/{target}/enable", token, new { expectedVersion = version });
        version = await VersionAsync(token, target);
        var enableThird = await SendAsync(HttpMethod.Post, $"/users/{target}/enable", token, new { expectedVersion = version });

        again.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(again)).ShouldBe("Identity.InvalidState");
        enableActive.StatusCode.ShouldBe(HttpStatusCode.OK);
        enableAgain.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        enableThird.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Disable_AndForceLogout_CannotBeUsedOnYourself()
    {
        var token = await LoginAsOperatorAsync("self-admin", _all);
        var me = (await JsonAsync(await GetAsync(NewClient(), "/api/v1/identity/me", token))).GetProperty("userId").GetGuid();
        var version = await VersionAsync(token, me);

        var disable = await SendAsync(HttpMethod.Post, $"/users/{me}/disable", token, new { expectedVersion = version, reason = (string?)null });
        var logout = await SendAsync(HttpMethod.Post, $"/users/{me}/force-logout", token, new { sessionId = (Guid?)null });

        disable.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CodeOfAsync(disable)).ShouldBe("Identity.CannotActOnSelf");
        logout.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        ActionsOf(await DetailAsync(token, me)).Contains("Disable", StringComparer.Ordinal).ShouldBeFalse();
        (await GetAsync(NewClient(), "/api/v1/identity/me", token)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_ReturnsATemporaryPasswordOnce_AndEverythingOldStopsWorking()
    {
        var token = await LoginAsOperatorAsync("reset-admin", _all);
        var target = await SeedMemberAsync("reset.target");
        var oldTokens = await LoginTokensAsync("reset.target");

        var response = await SendAsync(HttpMethod.Post, $"/users/{target}/reset-password", token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.CacheControl!.NoStore.ShouldBeTrue();
        var body = await JsonAsync(response);
        var temporary = body.GetProperty("temporaryPassword").GetString()!;
        temporary.Length.ShouldBeGreaterThanOrEqualTo(14);
        (await GetAsync(NewClient(), "/api/v1/identity/me", oldTokens.GetProperty("access_token").GetString())).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await PostAsync(NewClient(), "/api/v1/identity/auth/login", new { account = "reset.target", password = Password, clientType = "app" })).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var login = await LoginTokensAsync("reset.target", temporary);
        login.GetProperty("passwordChangeRequired").GetBoolean().ShouldBeTrue();
        (await GetAsync(NewClient(), "/api/v1/identity/users", login.GetProperty("access_token").GetString())).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // 临时密码不留在任何地方：幂等记录、安全事件里都找不到它，重置本身有一条安全事件
        (await WaitForSecurityEventsAsync($"\"Type\" = 'PasswordReset' AND \"ResourceId\" = '{target}'")).ShouldBe(1);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Identity"."IdempotencyRecord" WHERE "ResponseBody"::text LIKE '%{temporary}%'""")).ShouldBe(0);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Identity"."SecurityEvent" WHERE "Details"::text LIKE '%{temporary}%' OR "Reason" LIKE '%{temporary}%'""")).ShouldBe(0);
    }

    [Fact]
    public async Task ResetPassword_TwiceGivesDifferentPasswords_AndTheFirstOneNoLongerWorks()
    {
        var token = await LoginAsOperatorAsync("reset2-admin", _all);
        var target = await SeedMemberAsync("reset2.target");

        var first = (await JsonAsync(await SendAsync(HttpMethod.Post, $"/users/{target}/reset-password", token))).GetProperty("temporaryPassword").GetString()!;
        var second = (await JsonAsync(await SendAsync(HttpMethod.Post, $"/users/{target}/reset-password", token))).GetProperty("temporaryPassword").GetString()!;

        string.Equals(first, second, StringComparison.Ordinal).ShouldBeFalse();
        (await PostAsync(NewClient(), "/api/v1/identity/auth/login", new { account = "reset2.target", password = first, clientType = "app" })).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        await LoginTokensAsync("reset2.target", second);
    }

    [Fact]
    public async Task ResetPassword_OfAProtectedHolder_NeedsAProtectedRole()
    {
        var ordinary = await LoginAsOperatorAsync("reset-ordinary", _all);
        var guardian = await LoginAsOperatorAsync("reset-guardian", _all, isProtected: true);
        var target = await SeedMemberAsync("reset.protected", protectedRole: true);

        var denied = await SendAsync(HttpMethod.Post, $"/users/{target}/reset-password", ordinary);
        var allowed = await SendAsync(HttpMethod.Post, $"/users/{target}/reset-password", guardian);

        denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CodeOfAsync(denied)).ShouldBe("Identity.ProtectedRoleChangeDenied");
        allowed.StatusCode.ShouldBe(HttpStatusCode.OK);
        ActionsOf(await DetailAsync(ordinary, target)).Contains("ResetPassword", StringComparer.Ordinal).ShouldBeFalse();
        ActionsOf(await DetailAsync(guardian, target)).Contains("ResetPassword", StringComparer.Ordinal).ShouldBeTrue();
    }

    [Fact]
    public async Task UnlockLogin_LetsALockedAccountLogInAgain_AndTheDetailShowsTheLock()
    {
        var token = await LoginAsOperatorAsync("unlock-admin", _all);
        var target = await SeedMemberAsync("unlock.target");
        for (var i = 0; i < 3; i++)
        {
            await PostAsync(NewClient(), "/api/v1/identity/auth/login", new { account = "unlock.target", password = "wrong password here", clientType = "app" });
        }

        (await PostAsync(NewClient(), "/api/v1/identity/auth/login", new { account = "unlock.target", password = Password, clientType = "app" })).StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        (await DetailAsync(token, target)).GetProperty("loginLockedUntil").ValueKind.ShouldBe(JsonValueKind.String);

        var unlocked = await SendAsync(HttpMethod.Post, $"/users/{target}/unlock-login", token);

        unlocked.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await DetailAsync(token, target)).GetProperty("loginLockedUntil").ValueKind.ShouldBe(JsonValueKind.Null);
        await LoginTokensAsync("unlock.target");
        (await WaitForSecurityEventsAsync($"\"Type\" = 'AccountUnlocked' AND \"ResourceId\" = '{target}'")).ShouldBe(1);
    }

    [Fact]
    public async Task ForceLogout_EndsOneSessionOrAll_AndTheListShowsWhatIsActive()
    {
        var token = await LoginAsOperatorAsync("logout-admin", _all);
        var target = await SeedMemberAsync("logout.target");
        var phone = await LoginTokensAsync("logout.target");
        var laptop = await LoginTokensAsync("logout.target");
        var phoneToken = phone.GetProperty("access_token").GetString()!;
        var laptopToken = laptop.GetProperty("access_token").GetString()!;
        var sessions = await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{target}/sessions", token));
        sessions.GetArrayLength().ShouldBe(2);
        var firstId = sessions[0].GetProperty("id").GetGuid();

        var one = await SendAsync(HttpMethod.Post, $"/users/{target}/force-logout", token, new { sessionId = firstId });

        one.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{target}/sessions", token))).GetArrayLength().ShouldBe(1);
        (await DetailAsync(token, target)).GetProperty("activeSessionCount").GetInt32().ShouldBe(1);
        var statuses = new[] { await GetAsync(NewClient(), "/api/v1/identity/me", phoneToken), await GetAsync(NewClient(), "/api/v1/identity/me", laptopToken) };
        statuses.Count(r => r.StatusCode == HttpStatusCode.OK).ShouldBe(1);

        var all = await SendAsync(HttpMethod.Post, $"/users/{target}/force-logout", token, new { sessionId = (Guid?)null });

        all.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{target}/sessions", token))).GetArrayLength().ShouldBe(0);
        (await GetAsync(NewClient(), "/api/v1/identity/me", phoneToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await GetAsync(NewClient(), "/api/v1/identity/me", laptopToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForceLogout_WithASessionThatIsNotTheirs_IsNotFound_AndProtectedHoldersNeedAProtectedRole()
    {
        var ordinary = await LoginAsOperatorAsync("fl-ordinary", _all);
        var guardian = await LoginAsOperatorAsync("fl-guardian", _all, isProtected: true);
        var someone = await SeedMemberAsync("fl.someone");
        var admin = await SeedMemberAsync("fl.admin", protectedRole: true);
        await LoginTokensAsync("fl.someone");
        var otherSession = (await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{someone}/sessions", ordinary)))[0].GetProperty("id").GetGuid();
        await LoginTokensAsync("fl.admin");

        var wrongOwner = await SendAsync(HttpMethod.Post, $"/users/{admin}/force-logout", guardian, new { sessionId = otherSession });
        var denied = await SendAsync(HttpMethod.Post, $"/users/{admin}/force-logout", ordinary, new { sessionId = (Guid?)null });
        var allowed = await SendAsync(HttpMethod.Post, $"/users/{admin}/force-logout", guardian, new { sessionId = (Guid?)null });

        wrongOwner.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeOfAsync(wrongOwner)).ShouldBe("Identity.SessionNotFound");
        denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CodeOfAsync(denied)).ShouldBe("Identity.ProtectedRoleChangeDenied");
        allowed.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListUsers_FiltersByKeywordStatusAndRole_ShowsRolesAndLeavesOutTheSystemAccount()
    {
        var token = await LoginAsOperatorAsync("list-admin", _all);
        var sales = await Seed.RoleAsync("列表-销售", []);
        var support = await Seed.RoleAsync("列表-客服", []);
        var alice = await Seed.UserAsync("list.alice", Password, displayName: "列表艾丽丝", mobile: "13800138010");
        var bob = await Seed.UserAsync("list.bob", Password, displayName: "列表鲍勃", active: false);
        await Seed.GrantAsync(alice, sales);
        await Seed.GrantAsync(alice, support);
        await Seed.GrantAsync(bob, support);

        var byKeyword = await JsonAsync(await SendAsync(HttpMethod.Get, "/users?q=列表艾丽", token));
        var byMobile = await JsonAsync(await SendAsync(HttpMethod.Get, "/users?q=13800138010", token));
        var disabled = await JsonAsync(await SendAsync(HttpMethod.Get, "/users?q=list.&status=Disabled", token));
        var inSales = await JsonAsync(await SendAsync(HttpMethod.Get, $"/users?roleId={sales}", token));
        var sorted = await JsonAsync(await SendAsync(HttpMethod.Get, "/users?q=list.&sort=-account", token));
        var unknownStatus = await SendAsync(HttpMethod.Get, "/users?status=Archived", token);
        var everyone = await JsonAsync(await SendAsync(HttpMethod.Get, "/users?pageSize=200", token));

        byKeyword.GetProperty("items").EnumerateArray().Select(u => u.GetProperty("id").GetGuid()).ShouldBe([alice]);
        var row = byKeyword.GetProperty("items")[0];
        row.GetProperty("roles").EnumerateArray().Select(r => r.GetProperty("name").GetString()).Order(StringComparer.Ordinal).ShouldBe(["列表-客服", "列表-销售"]);
        byMobile.GetProperty("items").EnumerateArray().Select(u => u.GetProperty("id").GetGuid()).ShouldBe([alice]);
        disabled.GetProperty("items").EnumerateArray().Select(u => u.GetProperty("id").GetGuid()).ShouldBe([bob]);
        inSales.GetProperty("items").EnumerateArray().Select(u => u.GetProperty("id").GetGuid()).ShouldBe([alice]);
        sorted.GetProperty("items").EnumerateArray().Select(u => u.GetProperty("account").GetString()).ShouldBe(["list.bob", "list.alice"]);
        unknownStatus.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        everyone.GetProperty("items").EnumerateArray().Select(u => u.GetProperty("account").GetString()).Contains("system", StringComparer.Ordinal).ShouldBeFalse();
    }

    [Fact]
    public async Task GetUser_ShowsTheGrantsWithRoleNamesAndWhoGrantedThem()
    {
        var token = await LoginAsOperatorAsync("detail-admin", _all);
        var target = await SeedMemberAsync("detail.target");

        var detail = await DetailAsync(token, target);

        detail.GetProperty("hasPassword").GetBoolean().ShouldBeTrue();
        var grant = detail.GetProperty("grants").EnumerateArray().ShouldHaveSingleItem();
        grant.GetProperty("roleName").GetString().ShouldBe("成员-detail.target");
        grant.GetProperty("roleIsActive").GetBoolean().ShouldBeTrue();
        grant.GetProperty("roleIsProtected").GetBoolean().ShouldBeFalse();
        grant.GetProperty("grantedByName").GetString().ShouldNotBeNullOrWhiteSpace();
        grant.GetProperty("anchors").GetArrayLength().ShouldBe(0);
        (await SendAsync(HttpMethod.Get, $"/users/{Guid.CreateVersion7()}", token)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TheSystemAccount_IsShownButNothingCanBeDoneToIt()
    {
        var token = await LoginAsOperatorAsync("system-admin", _all);
        var system = new Guid("00000000-0000-0000-0000-000000000001");
        var detail = await DetailAsync(token, system);
        var version = detail.GetProperty("version").GetInt64();

        var edit = await SendAsync(HttpMethod.Put, $"/users/{system}", token, new { expectedVersion = version, account = "system", displayName = "改名", englishName = (string?)null, employeeNo = (string?)null, mobile = (string?)null, email = (string?)null });
        var reset = await SendAsync(HttpMethod.Post, $"/users/{system}/reset-password", token);
        var disable = await SendAsync(HttpMethod.Post, $"/users/{system}/disable", token, new { expectedVersion = version, reason = (string?)null });

        ActionsOf(detail).ShouldBeEmpty();
        foreach (var response in new[] { edit, reset, disable })
        {
            (await CodeOfAsync(response)).ShouldBe("Identity.SystemUserImmutable");
        }
    }

    [Fact]
    public async Task UserOptions_NeedOnlyALogin_AndNeverShowContactDetails()
    {
        var nobody = await LoginAsOperatorAsync("options-nobody", []);
        await Seed.UserAsync("options.target", Password, displayName: "选人张三", mobile: "13800138011");

        var response = await SendAsync(HttpMethod.Get, "/user-options?q=选人张", nobody);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var item = (await JsonAsync(response)).GetProperty("items").EnumerateArray().ShouldHaveSingleItem();
        item.GetProperty("displayName").GetString().ShouldBe("选人张三");
        item.GetProperty("status").GetString().ShouldBe("Active");
        item.TryGetProperty("mobile", out _).ShouldBeFalse();
        item.TryGetProperty("email", out _).ShouldBeFalse();
        item.TryGetProperty("account", out _).ShouldBeFalse();
        (await SendAsync(HttpMethod.Get, "/users", nobody)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
