using System.Net;
using System.Text.Json;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Identity.Application;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 模拟身份：只读与可写两种会话、不能模拟谁、结束与强制下线，端到端
/// </summary>
public sealed class ImpersonationTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private static readonly string[] _readOnly = [IdentityPermissions.UserView, IdentityPermissions.UserImpersonate];
    private static readonly string[] _canWrite = [IdentityPermissions.UserView, IdentityPermissions.UserImpersonate, IdentityPermissions.UserImpersonateWrite];

    // 被模拟的人：有角色管理的权限，写入的效果一眼能看出来
    private async Task<Guid> TargetAsync(string account, bool protectedRole = false, bool active = true, IReadOnlyCollection<string>? permissions = null)
    {
        var role = await Seed.RoleAsync("被模拟-" + account, permissions ?? [IdentityPermissions.RoleView, IdentityPermissions.RoleManage], isProtected: protectedRole);
        var user = await Seed.UserAsync(account, Password, displayName: "被模拟者 " + account, active: active);
        await Seed.GrantAsync(user, role);
        return user;
    }

    private async Task<HttpResponseMessage> StartAsync(string token, Guid target, string reason = "排查他看到的报价单为什么和我不一样", Guid? role = null) =>
        await SendAsync(HttpMethod.Post, $"/users/{target}/impersonation", token, new { roleId = role, reason });

    private async Task<string> ImpersonateAsync(string token, Guid target)
    {
        var response = await StartAsync(token, target);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await JsonAsync(response)).GetProperty("access_token").GetString()!;
    }

    private static async Task<string> CodeOfAsync(HttpResponseMessage response) => (await JsonAsync(response)).GetProperty("code").GetString()!;

    private async Task<HttpResponseMessage> CreateRoleAsync(string token, string name) =>
        await SendAsync(HttpMethod.Post, "/roles", token, new { name, description = (string?)null, sortOrder = 5, defaultScopeLevel = "Self" });

    [Fact]
    public async Task Start_GivesOnlyAnAccessToken_AndMeShowsWhoIsReallyActing()
    {
        var admin = await LoginAsOperatorAsync("imp-admin", _readOnly);
        var target = await TargetAsync("imp.target");
        var adminId = (await JsonAsync(await GetAsync(NewClient(), "/api/v1/identity/me", admin))).GetProperty("userId").GetGuid();

        var response = await StartAsync(admin, target);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.CacheControl!.NoStore.ShouldBeTrue();
        var tokens = await JsonAsync(response);
        tokens.GetProperty("refresh_token").ValueKind.ShouldBe(JsonValueKind.Null);
        tokens.GetProperty("expires_in").GetInt32().ShouldBeInRange(3500, 3600);
        var me = await JsonAsync(await GetAsync(NewClient(), "/api/v1/identity/me", tokens.GetProperty("access_token").GetString()));
        me.GetProperty("userId").GetGuid().ShouldBe(target);
        me.GetProperty("impersonating").GetBoolean().ShouldBeTrue();
        me.GetProperty("impersonatorUserId").GetGuid().ShouldBe(adminId);
        me.GetProperty("impersonationAllowsWrite").GetBoolean().ShouldBeFalse();
        (await GetAsync(NewClient(), "/api/v1/identity/me", admin)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await WaitForSecurityEventsAsync($"\"Type\" = 'ImpersonationStarted' AND \"ResourceId\" = '{target}' AND \"Reason\" = 'ReadOnly'")).ShouldBe(1);
    }

    [Fact]
    public async Task AReadOnlySession_CanReadWhatTheTargetCanRead_ButNoCommandPasses()
    {
        var admin = await LoginAsOperatorAsync("imp-ro-admin", _readOnly);
        var target = await TargetAsync("imp.ro.target");
        var theirs = await ImpersonateAsync(admin, target);

        var read = await SendAsync(HttpMethod.Get, "/roles", theirs);
        var write = await CreateRoleAsync(theirs, "只读会话里不该建成");

        read.StatusCode.ShouldBe(HttpStatusCode.OK);
        write.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CodeOfAsync(write)).ShouldBe("Common.ImpersonationReadOnly");
    }

    [Fact]
    public async Task WithTheWritePermission_TheSessionCanChangeData_AndMeSaysSo()
    {
        var admin = await LoginAsOperatorAsync("imp-rw-admin", _canWrite);
        var target = await TargetAsync("imp.rw.target");
        var theirs = await ImpersonateAsync(admin, target);

        var me = await JsonAsync(await GetAsync(NewClient(), "/api/v1/identity/me", theirs));
        var write = await CreateRoleAsync(theirs, "可写会话里建成的角色");

        me.GetProperty("impersonationAllowsWrite").GetBoolean().ShouldBeTrue();
        write.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await WaitForSecurityEventsAsync($"\"Type\" = 'ImpersonationStarted' AND \"ResourceId\" = '{target}' AND \"Reason\" = 'Write'")).ShouldBe(1);
    }

    [Fact]
    public async Task Start_NeedsAReason_AndTheImpersonatePermission()
    {
        var admin = await LoginAsOperatorAsync("imp-need-admin", _readOnly);
        var nobody = await LoginAsOperatorAsync("imp-need-nobody", [IdentityPermissions.UserView]);
        var target = await TargetAsync("imp.need.target");

        var noReason = await StartAsync(admin, target, reason: " ");
        var denied = await StartAsync(nobody, target);

        noReason.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CodeOfAsync(denied)).ShouldBe("Common.MissingPermission");
    }

    [Fact]
    public async Task YourselfTheSystemAccountAndProtectedHolders_CannotBeImpersonated_EvenByAProtectedAdmin()
    {
        var guardian = await LoginAsOperatorAsync("imp-guard", _canWrite, isProtected: true);
        var guardianId = (await JsonAsync(await GetAsync(NewClient(), "/api/v1/identity/me", guardian))).GetProperty("userId").GetGuid();
        var otherAdmin = await TargetAsync("imp.other.admin", protectedRole: true);
        var system = new Guid("00000000-0000-0000-0000-000000000001");

        foreach (var target in new[] { guardianId, otherAdmin, system })
        {
            var response = await StartAsync(guardian, target);

            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            (await CodeOfAsync(response)).ShouldBe("Identity.ImpersonationDenied");
        }
    }

    [Fact]
    public async Task ADisabledUserOrOneWithoutAUsableRole_CannotBeImpersonated()
    {
        var admin = await LoginAsOperatorAsync("imp-bad-admin", _readOnly);
        var disabled = await TargetAsync("imp.disabled.target", active: false);
        var onlyRole = await TargetAsync("imp.role.target");
        var stopped = await Seed.RoleAsync("被模拟-停用的角色", [], active: false);
        var noRoleUser = await Seed.UserAsync("imp.without.target", Password, displayName: "没有角色的人");
        await Seed.GrantAsync(noRoleUser, stopped);

        var disabledResponse = await StartAsync(admin, disabled);
        var noRoleResponse = await StartAsync(admin, noRoleUser);
        var wrongRole = await StartAsync(admin, onlyRole, role: Guid.CreateVersion7());

        disabledResponse.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(disabledResponse)).ShouldBe("Identity.InvalidState");
        noRoleResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CodeOfAsync(noRoleResponse)).ShouldBe("Identity.ImpersonationDenied");
        wrongRole.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CodeOfAsync(wrongRole)).ShouldBe("Identity.ImpersonationDenied");
    }

    [Fact]
    public async Task TheDetail_ShowsWhichKindOfImpersonationTheOperatorMayStart()
    {
        var readOnlyAdmin = await LoginAsOperatorAsync("imp-act-ro", _readOnly);
        var writeAdmin = await LoginAsOperatorAsync("imp-act-rw", _canWrite);
        var target = await TargetAsync("imp.act.target");
        var protectedTarget = await TargetAsync("imp.act.protected", protectedRole: true);

        async Task<string[]> ActionsAsync(string token, Guid user) => ActionsOf(await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{user}", token)));

        (await ActionsAsync(readOnlyAdmin, target)).ShouldBe(["Impersonate"]);
        (await ActionsAsync(writeAdmin, target)).ShouldBe(["Impersonate", "ImpersonateWrite"]);
        (await ActionsAsync(writeAdmin, protectedTarget)).ShouldBeEmpty();
    }

    [Fact]
    public async Task End_RevokesTheImpersonationSession_EvenWhenItIsReadOnly_AndTheAdminSessionStays()
    {
        var admin = await LoginAsOperatorAsync("imp-end-admin", _readOnly);
        var target = await TargetAsync("imp.end.target");
        var theirs = await ImpersonateAsync(admin, target);

        var notImpersonating = await SendAsync(HttpMethod.Delete, "/impersonation", admin);
        var ended = await SendAsync(HttpMethod.Delete, "/impersonation", theirs);

        notImpersonating.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(notImpersonating)).ShouldBe("Identity.NotImpersonating");
        ended.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await GetAsync(NewClient(), "/api/v1/identity/me", theirs)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await GetAsync(NewClient(), "/api/v1/identity/me", admin)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await WaitForSecurityEventsAsync($"\"Type\" = 'ImpersonationEnded' AND \"ResourceId\" = '{target}'")).ShouldBe(1);
    }

    [Fact]
    public async Task AnImpersonationSession_CannotStartAnotherOne()
    {
        var admin = await LoginAsOperatorAsync("imp-chain-admin", _canWrite);
        var middle = await TargetAsync("imp.chain.middle", permissions: _canWrite);
        var other = await TargetAsync("imp.chain.other");
        var theirs = await ImpersonateAsync(admin, middle);

        var response = await StartAsync(theirs, other);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CodeOfAsync(response)).ShouldBe("Identity.ImpersonationDenied");
    }

    [Fact]
    public async Task ForcingTheAdminOffline_AlsoEndsTheImpersonationSessionHeStarted()
    {
        var admin = await LoginAsOperatorAsync("imp-force-admin", _readOnly);
        var forcer = await LoginAsOperatorAsync("imp-force-by", [IdentityPermissions.UserView, IdentityPermissions.UserForceLogout]);
        var adminId = (await JsonAsync(await GetAsync(NewClient(), "/api/v1/identity/me", admin))).GetProperty("userId").GetGuid();
        var target = await TargetAsync("imp.force.target");
        var theirs = await ImpersonateAsync(admin, target);
        (await GetAsync(NewClient(), "/api/v1/identity/me", theirs)).StatusCode.ShouldBe(HttpStatusCode.OK);

        var forced = await SendAsync(HttpMethod.Post, $"/users/{adminId}/force-logout", forcer, new { sessionId = (Guid?)null });

        forced.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await GetAsync(NewClient(), "/api/v1/identity/me", theirs)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await GetAsync(NewClient(), "/api/v1/identity/me", admin)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
