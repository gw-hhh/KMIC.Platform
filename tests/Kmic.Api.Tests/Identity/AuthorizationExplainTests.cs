using System.Net;
using System.Text.Json;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Identity.Application;
using Kmic.Sample.Application;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

public sealed class AuthorizationExplainTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/identity/authorization/explain";

    private static readonly string[] _explain = [IdentityPermissions.AuthorizationExplain];
    private static readonly string[] _sampleView = [SamplePermissions.View];

    private async Task<(Guid East, Guid South)> TreeAsync()
    {
        var root = await Seed.OrgTreeAsync();
        var east = await UnitAsync(root, "华东");
        var south = await UnitAsync(root, "华南");
        await UnitAsync(east, "上海");
        // 访问上下文加载器按组织树版本号读取缓存，版本号为 0 时视为无组织树
        await Seed.SqlAsync("""UPDATE "Identity"."OrgTree" SET "ConcurrencyVersion" = "ConcurrencyVersion" + 1""");
        return (east, south);
    }

    // 同一测试类共用一个数据库，若组织单元已存在则直接复用
    private async Task<Guid> UnitAsync(Guid parent, string name)
    {
        var existing = await Seed.ScalarAsync<Guid?>("""SELECT "Id" FROM "Identity"."OrgUnit" WHERE "ParentId" = @parent AND "Name" = @name""", ("parent", parent), ("name", name));
        if (existing is { } found)
        {
            return found;
        }

        var id = Guid.CreateVersion7();
        await Seed.SqlAsync(
            """
            INSERT INTO "Identity"."OrgUnit" ("Id", "ParentId", "Name", "Code", "Status", "SortOrder", "CreatedAt", "CreatedBy", "TreeId")
            SELECT @id, @parent, @name, NULL, 'Active', 10, now(), "CreatedBy", "TreeId" FROM "Identity"."OrgUnit" WHERE "Id" = @parent
            """,
            ("id", id), ("parent", parent), ("name", name));
        return id;
    }

    private async Task OverrideAsync(Guid role, string scopeObject, string level) =>
        await Seed.SqlAsync(
            """
            INSERT INTO "Identity"."RoleDataScope" ("Id", "RoleId", "ScopeObject", "Level", "CreatedAt", "CreatedBy")
            VALUES (@id, @role, @scope, @level, now(), @role);
            UPDATE "Identity"."Role" SET "ConcurrencyVersion" = "ConcurrencyVersion" + 1 WHERE "Id" = @role
            """,
            ("id", Guid.CreateVersion7()), ("role", role), ("scope", scopeObject), ("level", level));

    private async Task AnchorAsync(Guid user, Guid role, Guid orgUnit) =>
        await Seed.SqlAsync(
            """
            INSERT INTO "Identity"."GrantAnchor" ("Id", "GrantId", "OrgUnitId", "CreatedAt", "CreatedBy")
            SELECT @id, g."Id", @unit, now(), @user FROM "Identity"."UserRoleGrant" g WHERE g."UserId" = @user AND g."RoleId" = @role
            """,
            ("id", Guid.CreateVersion7()), ("user", user), ("role", role), ("unit", orgUnit));

    private async Task PlaceAsync(Guid user, Guid orgUnit) =>
        await Seed.SqlAsync("""UPDATE "Identity"."User" SET "OrgUnitId" = @unit WHERE "Id" = @user""", ("unit", orgUnit), ("user", user));

    private async Task<HttpResponseMessage> ExplainAsync(string token, Guid user, Guid role, string? scopeObject = null) =>
        await SendAsync(HttpMethod.Get, scopeObject is null ? $"{Root}?userId={user}&roleId={role}" : $"{Root}?userId={user}&roleId={role}&scopeObject={scopeObject}", token);

    private static string[] NamesOf(JsonElement anchors) => [.. anchors.EnumerateArray().Select(a => a.GetProperty("name").GetString()!)];

    [Fact]
    public async Task ALevelSetForTheScopeObject_WithGrantAnchors_IsExplainedWithItsSources()
    {
        var token = await LoginAsOperatorAsync("explain-admin", _explain);
        var (east, south) = await TreeAsync();
        var role = await Seed.RoleAsync("区域经理", _sampleView, scope: "Self");
        await OverrideAsync(role, SampleScopes.Specimen, "OrgUnitAndBelow");
        var user = await Seed.UserAsync("explain-manager", Password, displayName: "张三");
        await PlaceAsync(user, south);
        await Seed.GrantAsync(user, role);
        await AnchorAsync(user, role, east);

        var withScope = await ExplainAsync(token, user, role, SampleScopes.Specimen);
        var withoutScope = await ExplainAsync(token, user, role);

        withScope.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await JsonAsync(withScope);
        body.GetProperty("userName").GetString().ShouldBe("张三");
        body.GetProperty("roleName").GetString().ShouldBe("区域经理");
        body.GetProperty("usable").GetBoolean().ShouldBeTrue();
        body.GetProperty("unusableReason").ValueKind.ShouldBe(JsonValueKind.Null);
        body.GetProperty("permissions").EnumerateArray().Select(p => p.GetString()).ShouldBe([SamplePermissions.View]);
        var scope = body.GetProperty("dataScope");
        scope.GetProperty("scopeObject").GetString().ShouldBe(SampleScopes.Specimen);
        scope.GetProperty("displayName").GetString().ShouldBe("示例");
        scope.GetProperty("level").GetString().ShouldBe("OrgUnitAndBelow");
        scope.GetProperty("levelOverridden").GetBoolean().ShouldBeTrue("角色默认数据范围为本人，但为示例模块单独覆盖为本组织及下级");
        NamesOf(scope.GetProperty("anchors")).ShouldBe(["华东"], "角色授予已指定锚点，应使用锚点华东而非用户所属的华南");
        scope.GetProperty("anchorsFromGrant").GetBoolean().ShouldBeTrue();
        scope.GetProperty("orgUnitCount").GetInt32().ShouldBe(2, "应包含华东及其下级上海共 2 个组织单元");
        (await JsonAsync(withoutScope)).GetProperty("dataScope").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task TheRoleDefault_WithoutAnchors_StartsFromTheUsersOwnOrgUnit()
    {
        var token = await LoginAsOperatorAsync("explain-default-admin", _explain);
        var (_, south) = await TreeAsync();
        var role = await Seed.RoleAsync("门店主管", _sampleView, scope: "OrgUnit");
        var user = await Seed.UserAsync("explain-store", Password);
        await PlaceAsync(user, south);
        await Seed.GrantAsync(user, role);

        var scope = (await JsonAsync(await ExplainAsync(token, user, role, SampleScopes.Specimen))).GetProperty("dataScope");

        scope.GetProperty("level").GetString().ShouldBe("OrgUnit");
        scope.GetProperty("levelOverridden").GetBoolean().ShouldBeFalse();
        NamesOf(scope.GetProperty("anchors")).ShouldBe(["华南"]);
        scope.GetProperty("anchorsFromGrant").GetBoolean().ShouldBeFalse();
        scope.GetProperty("orgUnitCount").GetInt32().ShouldBe(1);
    }

    [Fact]
    public async Task SelfAndAll_AreNotJudgedByOrgUnit()
    {
        var token = await LoginAsOperatorAsync("explain-self-admin", _explain);
        var self = await Seed.RoleAsync("只看自己", _sampleView, scope: "Self");
        var all = await Seed.RoleAsync("看全部", _sampleView, scope: "All");
        var user = await Seed.UserAsync("explain-self", Password);
        await Seed.GrantAsync(user, self);
        await Seed.GrantAsync(user, all);

        foreach (var (role, level) in new[] { (self, "Self"), (all, "All") })
        {
            var scope = (await JsonAsync(await ExplainAsync(token, user, role, SampleScopes.Specimen))).GetProperty("dataScope");
            scope.GetProperty("level").GetString().ShouldBe(level);
            scope.GetProperty("anchors").GetArrayLength().ShouldBe(0);
            scope.GetProperty("anchorsFromGrant").GetBoolean().ShouldBeFalse();
            scope.GetProperty("orgUnitCount").ValueKind.ShouldBe(JsonValueKind.Null);
        }
    }

    [Fact]
    public async Task WhenTheUserCannotUseTheRole_TheReasonIsGiven_AndNothingElse()
    {
        var token = await LoginAsOperatorAsync("explain-reason-admin", _explain);
        var role = await Seed.RoleAsync("会停用的角色", _sampleView);
        var otherRole = await Seed.RoleAsync("没授予的角色", _sampleView);
        var disabled = await Seed.UserAsync("explain-disabled", Password, active: false);
        var active = await Seed.UserAsync("explain-active", Password);
        await Seed.GrantAsync(disabled, role);
        await Seed.GrantAsync(active, role);

        var reasons = new List<string?>();
        foreach (var (user, roleId) in new[] { (disabled, role), (active, otherRole) })
        {
            var body = await JsonAsync(await ExplainAsync(token, user, roleId, SampleScopes.Specimen));
            body.GetProperty("usable").GetBoolean().ShouldBeFalse();
            body.GetProperty("permissions").GetArrayLength().ShouldBe(0);
            body.GetProperty("dataScope").ValueKind.ShouldBe(JsonValueKind.Null);
            reasons.Add(body.GetProperty("unusableReason").GetString());
        }

        await Seed.SetRoleStatusAsync(role, active: false);
        reasons.Add((await JsonAsync(await ExplainAsync(token, active, role))).GetProperty("unusableReason").GetString());

        reasons.ShouldBe(["用户已禁用", "用户未被授予该角色", "角色已停用"]);
    }

    [Fact]
    public async Task UnknownTargets_AndMissingPermission_AreRefused()
    {
        var token = await LoginAsOperatorAsync("explain-refuse-admin", _explain);
        var plain = await LoginAsOperatorAsync("explain-refuse-plain", []);
        var role = await Seed.RoleAsync("某个角色", _sampleView);
        var user = await Seed.UserAsync("explain-refuse-user", Password);

        var unknownScope = await ExplainAsync(token, user, role, "Sample.Nothing");
        var unknownUser = await ExplainAsync(token, Guid.CreateVersion7(), role);
        var unknownRole = await ExplainAsync(token, user, Guid.CreateVersion7());
        var forbidden = await ExplainAsync(plain, user, role);

        unknownScope.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await JsonAsync(unknownScope)).GetProperty("code").GetString().ShouldBe("Identity.ScopeObjectUnknown");
        unknownUser.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await JsonAsync(unknownUser)).GetProperty("code").GetString().ShouldBe("Identity.UserNotFound");
        unknownRole.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await JsonAsync(unknownRole)).GetProperty("code").GetString().ShouldBe("Identity.RoleNotFound");
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
