using System.Net;
using System.Text.Json;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Identity.Application;
using Kmic.Sample.Application;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 角色授予与锚点：授予、收回、改锚点，以及不能改自己的授予、受保护角色只能由受保护角色的持有人授予这些规则，端到端
/// </summary>
public sealed class GrantAdministrationTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private static readonly string[] _grantAdmin = [IdentityPermissions.UserView, IdentityPermissions.UserGrant, IdentityPermissions.OrgUnitManage];

    private async Task<JsonElement> DetailAsync(string token, Guid user)
    {
        var response = await SendAsync(HttpMethod.Get, $"/users/{user}", token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await JsonAsync(response);
    }

    private async Task<long> VersionAsync(string token, Guid user) => (await DetailAsync(token, user)).GetProperty("version").GetInt64();

    private async Task<HttpResponseMessage> GrantAsync(string token, Guid user, Guid role, params Guid[] anchors) =>
        await SendAsync(HttpMethod.Post, $"/users/{user}/grants", token, new { expectedVersion = await VersionAsync(token, user), roleId = role, anchors });

    private async Task<HttpResponseMessage> RevokeAsync(string token, Guid user, Guid role) =>
        await SendAsync(HttpMethod.Post, $"/users/{user}/grants/{role}/revoke", token, new { expectedVersion = await VersionAsync(token, user) });

    private async Task<HttpResponseMessage> SetAnchorsAsync(string token, Guid user, Guid role, params Guid[] anchors) =>
        await SendAsync(HttpMethod.Put, $"/users/{user}/grants/{role}/anchors", token, new { expectedVersion = await VersionAsync(token, user), anchors });

    private async Task<Guid> AddUnitAsync(string token, Guid parent, string name)
    {
        var tree = await JsonAsync(await SendAsync(HttpMethod.Get, "/org-tree", token));
        var response = await SendAsync(HttpMethod.Post, "/org-units", token, new { expectedVersion = tree.GetProperty("version").GetInt64(), parentId = parent, name, code = (string?)null });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await JsonAsync(response)).GetProperty("id").GetGuid();
    }

    private async Task<Guid> MemberAsync(string account, Guid? role = null)
    {
        var user = await Seed.UserAsync(account, Password, displayName: "成员 " + account);
        await Seed.GrantAsync(user, role ?? await Seed.RoleAsync("起始角色-" + account, [SamplePermissions.View]));
        return user;
    }

    private static async Task<string> CodeOfAsync(HttpResponseMessage response) => (await JsonAsync(response)).GetProperty("code").GetString()!;

    [Fact]
    public async Task Grant_AddsTheRole_RecordsWhoGrantedIt_AndWritesASecurityEvent()
    {
        var admin = await LoginAsOperatorAsync("grant-admin", _grantAdmin);
        var member = await MemberAsync("grant.member");
        var role = await Seed.RoleAsync("授予-销售经理", [SamplePermissions.View, SamplePermissions.Create]);

        var response = await GrantAsync(admin, member, role);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var grants = (await DetailAsync(admin, member)).GetProperty("grants").EnumerateArray().ToList();
        grants.Count.ShouldBe(2);
        var granted = grants.Single(g => g.GetProperty("roleId").GetGuid() == role);
        granted.GetProperty("roleName").GetString().ShouldBe("授予-销售经理");
        granted.GetProperty("grantedByName").GetString().ShouldBe("操作者 grant-admin");
        (await WaitForSecurityEventsAsync($"\"Type\" = 'AuthorizationConfigChanged' AND \"Reason\" = 'RoleGranted' AND \"ResourceId\" = '{member}' AND \"Details\"->>'roleId' = '{role}'")).ShouldBe(1);
    }

    [Fact]
    public async Task Grant_TheSameRoleAgain_ADisabledRole_OrAnUnknownRole_IsRefused()
    {
        var admin = await LoginAsOperatorAsync("grant-bad-admin", _grantAdmin);
        var member = await MemberAsync("grant.bad.member");
        var role = await Seed.RoleAsync("授予-重复", []);
        var disabled = await Seed.RoleAsync("授予-已停用", [], active: false);
        (await GrantAsync(admin, member, role)).StatusCode.ShouldBe(HttpStatusCode.OK);

        var again = await GrantAsync(admin, member, role);
        var stopped = await GrantAsync(admin, member, disabled);
        var unknown = await GrantAsync(admin, member, Guid.CreateVersion7());

        again.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(again)).ShouldBe("Identity.GrantExists");
        stopped.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(stopped)).ShouldBe("Identity.RoleDisabled");
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeOfAsync(unknown)).ShouldBe("Identity.RoleNotFound");
    }

    [Fact]
    public async Task Anchors_AreShownByName_CanBeReplacedOrCleared_AndMustBeEnabledUnits()
    {
        var root = await Seed.OrgTreeAsync();
        var admin = await LoginAsOperatorAsync("grant-anchor-admin", _grantAdmin);
        var member = await MemberAsync("grant.anchor.member");
        var role = await Seed.RoleAsync("锚点-区域经理", []);
        var east = await AddUnitAsync(admin, root, "锚点-华东");
        var south = await AddUnitAsync(admin, root, "锚点-华南");
        var closed = await AddUnitAsync(admin, root, "锚点-已撤销");
        var tree = await JsonAsync(await SendAsync(HttpMethod.Get, "/org-tree", admin));
        (await SendAsync(HttpMethod.Post, $"/org-units/{closed}/disable", admin, new { expectedVersion = tree.GetProperty("version").GetInt64() })).StatusCode.ShouldBe(HttpStatusCode.OK);

        var granted = await GrantAsync(admin, member, role, east, south);
        var onClosed = await SetAnchorsAsync(admin, member, role, closed);
        var onUnknown = await SetAnchorsAsync(admin, member, role, Guid.CreateVersion7());
        var replaced = await SetAnchorsAsync(admin, member, role, south);

        granted.StatusCode.ShouldBe(HttpStatusCode.OK);
        onClosed.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(onClosed)).ShouldBe("Identity.OrgUnitDisabled");
        onUnknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeOfAsync(onUnknown)).ShouldBe("Identity.OrgUnitNotFound");
        replaced.StatusCode.ShouldBe(HttpStatusCode.OK);
        var anchors = AnchorsOf(await DetailAsync(admin, member), role);
        anchors.ShouldBe(["锚点-华南"]);

        (await SetAnchorsAsync(admin, member, role)).StatusCode.ShouldBe(HttpStatusCode.OK);
        AnchorsOf(await DetailAsync(admin, member), role).ShouldBeEmpty();
    }

    private static string[] AnchorsOf(JsonElement detail, Guid role) =>
        [.. detail.GetProperty("grants").EnumerateArray().Single(g => g.GetProperty("roleId").GetGuid() == role).GetProperty("anchors").EnumerateArray().Select(a => a.GetProperty("name").GetString()!)];

    [Fact]
    public async Task SettingAnchors_OfARoleTheUserDoesNotHave_IsNotFound()
    {
        var admin = await LoginAsOperatorAsync("grant-missing-admin", _grantAdmin);
        var member = await MemberAsync("grant.missing.member");
        var other = await Seed.RoleAsync("锚点-没授予过", []);

        var response = await SetAnchorsAsync(admin, member, other);
        var revoke = await RevokeAsync(admin, member, other);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeOfAsync(response)).ShouldBe("Identity.GrantNotFound");
        (await CodeOfAsync(revoke)).ShouldBe("Identity.GrantNotFound");
    }

    [Fact]
    public async Task Revoke_RemovesTheRole_AndTheirSessionLosesItsPermissions()
    {
        var admin = await LoginAsOperatorAsync("revoke-admin", _grantAdmin);
        var member = await MemberAsync("revoke.member");
        var tokens = await LoginTokensAsync("revoke.member");
        var access = tokens.GetProperty("access_token").GetString()!;
        var before = await JsonAsync(await GetAsync(NewClient(), "/api/v1/identity/me", access));
        before.GetProperty("permissions").GetArrayLength().ShouldBe(1);
        var starting = (await DetailAsync(admin, member)).GetProperty("grants")[0].GetProperty("roleId").GetGuid();

        var revoked = await RevokeAsync(admin, member, starting);

        revoked.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await DetailAsync(admin, member)).GetProperty("grants").GetArrayLength().ShouldBe(0);
        var after = await JsonAsync(await GetAsync(NewClient(), "/api/v1/identity/me", access));
        after.GetProperty("permissions").GetArrayLength().ShouldBe(0);
        (await WaitForSecurityEventsAsync($"\"Type\" = 'AuthorizationConfigChanged' AND \"Reason\" = 'RoleRevoked' AND \"ResourceId\" = '{member}'")).ShouldBe(1);
    }

    [Fact]
    public async Task NobodyCanChangeTheirOwnGrants()
    {
        var admin = await LoginAsOperatorAsync("self-grant-admin", _grantAdmin);
        var me = (await JsonAsync(await GetAsync(NewClient(), "/api/v1/identity/me", admin))).GetProperty("userId").GetGuid();
        var mine = (await DetailAsync(admin, me)).GetProperty("grants")[0].GetProperty("roleId").GetGuid();
        var another = await Seed.RoleAsync("自己-想要的角色", []);

        var grant = await GrantAsync(admin, me, another);
        var revoke = await RevokeAsync(admin, me, mine);
        var anchors = await SetAnchorsAsync(admin, me, mine);

        foreach (var response in new[] { grant, revoke, anchors })
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            (await CodeOfAsync(response)).ShouldBe("Identity.CannotChangeOwnGrants");
        }
    }

    [Fact]
    public async Task AProtectedRole_CanOnlyBeGrantedRevokedAndAnchoredByAProtectedRoleHolder()
    {
        var root = await Seed.OrgTreeAsync();
        var ordinary = await LoginAsOperatorAsync("prot-ordinary", _grantAdmin);
        var guardian = await LoginAsOperatorAsync("prot-guardian", _grantAdmin, isProtected: true);
        var member = await MemberAsync("prot.member");
        var protectedRole = await Seed.RoleAsync("受保护的财务总监", [SamplePermissions.View], isProtected: true);
        var unit = await AddUnitAsync(guardian, root, "受保护-部门");

        var denied = await GrantAsync(ordinary, member, protectedRole);
        var allowed = await GrantAsync(guardian, member, protectedRole);
        var deniedAnchors = await SetAnchorsAsync(ordinary, member, protectedRole, unit);
        var deniedRevoke = await RevokeAsync(ordinary, member, protectedRole);
        var allowedRevoke = await RevokeAsync(guardian, member, protectedRole);

        denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CodeOfAsync(denied)).ShouldBe("Identity.ProtectedRoleChangeDenied");
        allowed.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await CodeOfAsync(deniedAnchors)).ShouldBe("Identity.ProtectedRoleChangeDenied");
        (await CodeOfAsync(deniedRevoke)).ShouldBe("Identity.ProtectedRoleChangeDenied");
        allowedRevoke.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangingGrants_NeedsTheGrantPermission_AndAStaleVersionIsAConflict()
    {
        var viewer = await LoginAsOperatorAsync("grant-viewer", [IdentityPermissions.UserView]);
        var admin = await LoginAsOperatorAsync("grant-stale-admin", _grantAdmin);
        var member = await MemberAsync("grant.stale.member");
        var role = await Seed.RoleAsync("授予-权限测试", []);
        var version = await VersionAsync(admin, member);

        var denied = await GrantAsync(viewer, member, role);
        (await GrantAsync(admin, member, role)).StatusCode.ShouldBe(HttpStatusCode.OK);
        var stale = await SendAsync(HttpMethod.Post, $"/users/{member}/grants", admin, new { expectedVersion = version, roleId = await Seed.RoleAsync("授予-另一个", []), anchors = Array.Empty<Guid>() });

        denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(stale)).ShouldBe("Common.VersionConflict");
    }
}
