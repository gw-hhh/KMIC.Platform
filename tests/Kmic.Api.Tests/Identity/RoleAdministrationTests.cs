using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Identity.Application;
using Kmic.Sample.Application;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 角色管理：增改、权限点与数据范围、停用启用，以及受保护角色、内置角色、版本冲突这些规则，端到端
/// </summary>
public sealed class RoleAdministrationTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Password = "correct horse battery";

    private static readonly string[] _manageRoles = [IdentityPermissions.RoleView, IdentityPermissions.RoleManage];

    private static Dictionary<string, string> Overrides(string scopeObject, string level) => new(StringComparer.Ordinal) { [scopeObject] = level };

    private static string[] ActionsOf(JsonElement detail) =>
        [.. detail.GetProperty("allowedActions").EnumerateArray().Select(a => a.GetString()!).Order(StringComparer.Ordinal)];

    private async Task<string> LoginAsOperatorAsync(string account, IReadOnlyCollection<string> permissions, bool isProtected = false)
    {
        var role = await Seed.RoleAsync("操作者-" + account, permissions, isProtected: isProtected);
        var user = await Seed.UserAsync(account, Password);
        await Seed.GrantAsync(user, role);
        var login = await PostAsync(NewClient(), "/api/v1/identity/auth/login", new { account, password = Password, clientType = "app" });
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await JsonAsync(login)).GetProperty("access_token").GetString()!;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string token, object? body = null)
    {
        using var request = new HttpRequestMessage(method, "/api/v1/identity" + url) { Content = body is null ? null : JsonContent.Create(body) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (method != HttpMethod.Get)
        {
            request.Headers.Add("Idempotency-Key", Guid.CreateVersion7().ToString());
        }

        return await NewClient().SendAsync(request, Ct);
    }

    private async Task<Guid> CreateRoleAsync(string token, string name, string scope = "Self")
    {
        var response = await SendAsync(HttpMethod.Post, "/roles", token, new { name, description = (string?)null, sortOrder = 20, defaultScopeLevel = scope });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await JsonAsync(response)).GetProperty("id").GetGuid();
    }

    private async Task<JsonElement> DetailAsync(string token, Guid id)
    {
        var response = await SendAsync(HttpMethod.Get, $"/roles/{id}", token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await JsonAsync(response);
    }

    [Fact]
    public async Task CreateRole_ReturnsItsLocation_AndTheDetailShowsNoPermissions()
    {
        var token = await LoginAsOperatorAsync("create-admin", _manageRoles);

        var response = await SendAsync(HttpMethod.Post, "/roles", token, new { name = "客服", description = "只看不改", sortOrder = 30, defaultScopeLevel = "OrgUnit" });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await JsonAsync(response);
        var id = created.GetProperty("id").GetGuid();
        response.Headers.Location!.ToString().ShouldEndWith($"/api/v1/identity/roles/{id}");
        var detail = await DetailAsync(token, id);
        detail.GetProperty("name").GetString().ShouldBe("客服");
        detail.GetProperty("description").GetString().ShouldBe("只看不改");
        detail.GetProperty("status").GetString().ShouldBe("Active");
        detail.GetProperty("defaultScopeLevel").GetString().ShouldBe("OrgUnit");
        detail.GetProperty("isProtected").GetBoolean().ShouldBeFalse();
        detail.GetProperty("isBuiltIn").GetBoolean().ShouldBeFalse();
        detail.GetProperty("permissions").GetArrayLength().ShouldBe(0);
        detail.GetProperty("holderCount").GetInt32().ShouldBe(0);
        ActionsOf(detail).ShouldBe(["Disable", "Edit", "Enable", "SetDataScopes", "SetPermissions"]);
    }

    [Fact]
    public async Task CreateRole_WithTheSameNameInAnotherCase_IsRejected()
    {
        var token = await LoginAsOperatorAsync("dup-admin", _manageRoles);
        await CreateRoleAsync(token, "Auditor");

        var response = await SendAsync(HttpMethod.Post, "/roles", token, new { name = "AUDITOR", description = (string?)null, sortOrder = 1, defaultScopeLevel = "Self" });

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Identity.RoleNameExists");
    }

    [Fact]
    public async Task CreateRole_WithAnEmptyName_IsAValidationError()
    {
        var token = await LoginAsOperatorAsync("blank-admin", _manageRoles);

        var response = await SendAsync(HttpMethod.Post, "/roles", token, new { name = "  ", description = (string?)null, sortOrder = 1, defaultScopeLevel = "Self" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ManagingRoles_WithoutTheManagePermission_IsForbidden()
    {
        var viewer = await LoginAsOperatorAsync("viewer-only", [IdentityPermissions.RoleView]);
        var manager = await LoginAsOperatorAsync("viewer-target-admin", _manageRoles);
        var id = await CreateRoleAsync(manager, "只读用的角色");

        var create = await SendAsync(HttpMethod.Post, "/roles", viewer, new { name = "不该成功", description = (string?)null, sortOrder = 1, defaultScopeLevel = "Self" });
        var list = await SendAsync(HttpMethod.Get, "/roles", viewer);
        var detail = await SendAsync(HttpMethod.Get, $"/roles/{id}", viewer);

        create.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        list.StatusCode.ShouldBe(HttpStatusCode.OK);
        detail.StatusCode.ShouldBe(HttpStatusCode.OK);
        ActionsOf(await JsonAsync(detail)).ShouldBeEmpty();
    }

    [Fact]
    public async Task SetPermissions_ReplacesTheWholeSet_AndBumpsTheVersion()
    {
        var token = await LoginAsOperatorAsync("perm-admin", _manageRoles);
        var id = await CreateRoleAsync(token, "销售主管");
        var before = await DetailAsync(token, id);

        var first = await SendAsync(HttpMethod.Put, $"/roles/{id}/permissions", token, new { expectedVersion = before.GetProperty("version").GetInt64(), permissions = new[] { SamplePermissions.View, SamplePermissions.Create } });

        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        var afterFirst = await DetailAsync(token, id);
        string.Join(",", afterFirst.GetProperty("permissions").EnumerateArray().Select(p => p.GetProperty("code").GetString())).ShouldBe($"{SamplePermissions.Create},{SamplePermissions.View}");
        afterFirst.GetProperty("version").GetInt64().ShouldBeGreaterThan(before.GetProperty("version").GetInt64());

        var second = await SendAsync(HttpMethod.Put, $"/roles/{id}/permissions", token, new { expectedVersion = afterFirst.GetProperty("version").GetInt64(), permissions = new[] { SamplePermissions.View } });

        second.StatusCode.ShouldBe(HttpStatusCode.OK);
        var afterSecond = await DetailAsync(token, id);
        afterSecond.GetProperty("permissions").EnumerateArray().Select(p => p.GetProperty("code").GetString()).ShouldBe([SamplePermissions.View]);
    }

    [Fact]
    public async Task SetPermissions_WithAnUnknownCode_IsRejected_AndNothingChanges()
    {
        var token = await LoginAsOperatorAsync("unknown-admin", _manageRoles);
        var id = await CreateRoleAsync(token, "拼错权限点的角色");
        var before = await DetailAsync(token, id);

        var response = await SendAsync(HttpMethod.Put, $"/roles/{id}/permissions", token, new { expectedVersion = before.GetProperty("version").GetInt64(), permissions = new[] { SamplePermissions.View, "Sample.Nothing.Here" } });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await JsonAsync(response);
        problem.GetProperty("code").GetString().ShouldBe("Identity.PermissionUnknown");
        problem.GetProperty("detail").GetString()!.ShouldContain("Sample.Nothing.Here");
        (await DetailAsync(token, id)).GetProperty("permissions").GetArrayLength().ShouldBe(0);
    }

    [Fact]
    public async Task SetPermissions_WithAStaleVersion_ReturnsAConflict()
    {
        var token = await LoginAsOperatorAsync("stale-admin", _manageRoles);
        var id = await CreateRoleAsync(token, "两个人同时改的角色");
        var seen = (await DetailAsync(token, id)).GetProperty("version").GetInt64();
        (await SendAsync(HttpMethod.Put, $"/roles/{id}", token, new { expectedVersion = seen, name = "已被别人改名", description = (string?)null, sortOrder = 20 })).StatusCode.ShouldBe(HttpStatusCode.OK);

        var response = await SendAsync(HttpMethod.Put, $"/roles/{id}/permissions", token, new { expectedVersion = seen, permissions = new[] { SamplePermissions.View } });

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Common.VersionConflict");
    }

    [Fact]
    public async Task SetDataScopes_AcceptsCatalogObjects_AndRejectsUnknownOnes()
    {
        var token = await LoginAsOperatorAsync("scope-admin", _manageRoles);
        var id = await CreateRoleAsync(token, "区域经理");
        var version = (await DetailAsync(token, id)).GetProperty("version").GetInt64();

        var unknown = await SendAsync(HttpMethod.Put, $"/roles/{id}/data-scopes", token, new { expectedVersion = version, defaultLevel = "OrgUnit", overrides = Overrides("Sample.Nothing", "All") });
        var ok = await SendAsync(HttpMethod.Put, $"/roles/{id}/data-scopes", token, new { expectedVersion = version, defaultLevel = "OrgUnit", overrides = Overrides(SampleScopes.Specimen, "OrgUnitAndBelow") });

        unknown.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await JsonAsync(unknown)).GetProperty("code").GetString().ShouldBe("Identity.ScopeObjectUnknown");
        ok.StatusCode.ShouldBe(HttpStatusCode.OK);
        var detail = await DetailAsync(token, id);
        detail.GetProperty("defaultScopeLevel").GetString().ShouldBe("OrgUnit");
        var scope = detail.GetProperty("dataScopes").EnumerateArray().ShouldHaveSingleItem();
        scope.GetProperty("scopeObject").GetString().ShouldBe(SampleScopes.Specimen);
        scope.GetProperty("level").GetString().ShouldBe("OrgUnitAndBelow");
    }

    [Fact]
    public async Task BuiltInRole_CannotBeDisabled_AndItsPermissionsCannotBeEdited()
    {
        var role = await Seed.RoleAsync("系统管理员", [], isProtected: true, builtIn: true);
        var user = await Seed.UserAsync("root-admin", Password);
        await Seed.GrantAsync(user, role);
        var login = await PostAsync(NewClient(), "/api/v1/identity/auth/login", new { account = "root-admin", password = Password, clientType = "app" });
        var token = (await JsonAsync(login)).GetProperty("access_token").GetString()!;
        var version = (await DetailAsync(token, role)).GetProperty("version").GetInt64();

        var setPermissions = await SendAsync(HttpMethod.Put, $"/roles/{role}/permissions", token, new { expectedVersion = version, permissions = new[] { SamplePermissions.View } });
        var disable = await SendAsync(HttpMethod.Post, $"/roles/{role}/disable", token, new { expectedVersion = version, reason = "试一下" });
        var rename = await SendAsync(HttpMethod.Put, $"/roles/{role}", token, new { expectedVersion = version, name = "超级管理员", description = (string?)null, sortOrder = 1 });

        setPermissions.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await JsonAsync(setPermissions)).GetProperty("code").GetString().ShouldBe("Identity.SystemRoleImmutable");
        disable.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await JsonAsync(disable)).GetProperty("code").GetString().ShouldBe("Identity.SystemRoleImmutable");
        rename.StatusCode.ShouldBe(HttpStatusCode.OK);
        ActionsOf(await DetailAsync(token, role)).ShouldBe(["Edit", "Enable"]);
    }

    [Fact]
    public async Task ProtectedRole_CanOnlyBeManagedByAHolderOfAProtectedRole()
    {
        var protectedRole = await Seed.RoleAsync("财务总监", [SamplePermissions.View], isProtected: true);
        var ordinary = await LoginAsOperatorAsync("ordinary-admin", _manageRoles);
        var guardian = await LoginAsOperatorAsync("guardian-admin", _manageRoles, isProtected: true);
        var version = (await DetailAsync(ordinary, protectedRole)).GetProperty("version").GetInt64();

        var denied = await SendAsync(HttpMethod.Put, $"/roles/{protectedRole}/permissions", ordinary, new { expectedVersion = version, permissions = new[] { SamplePermissions.View, SamplePermissions.Create } });
        var allowed = await SendAsync(HttpMethod.Put, $"/roles/{protectedRole}/permissions", guardian, new { expectedVersion = version, permissions = new[] { SamplePermissions.View, SamplePermissions.Create } });

        denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await JsonAsync(denied)).GetProperty("code").GetString().ShouldBe("Identity.ProtectedRoleChangeDenied");
        allowed.StatusCode.ShouldBe(HttpStatusCode.OK);
        ActionsOf(await DetailAsync(ordinary, protectedRole)).ShouldBeEmpty();
    }

    [Fact]
    public async Task DisableAndEnable_ChangeTheStatus_AndTheListCanFilterByIt()
    {
        var token = await LoginAsOperatorAsync("toggle-admin", _manageRoles);
        var id = await CreateRoleAsync(token, "临时项目组");
        var version = (await DetailAsync(token, id)).GetProperty("version").GetInt64();

        var disabled = await SendAsync(HttpMethod.Post, $"/roles/{id}/disable", token, new { expectedVersion = version, reason = "项目结束" });

        disabled.StatusCode.ShouldBe(HttpStatusCode.OK);
        var asDisabled = await DetailAsync(token, id);
        asDisabled.GetProperty("status").GetString().ShouldBe("Disabled");
        var filtered = await JsonAsync(await SendAsync(HttpMethod.Get, "/roles?status=Disabled&q=临时", token));
        filtered.GetProperty("items").EnumerateArray().Select(r => r.GetProperty("id").GetGuid()).ShouldBe([id]);

        var enabled = await SendAsync(HttpMethod.Post, $"/roles/{id}/enable", token, new { expectedVersion = asDisabled.GetProperty("version").GetInt64() });

        enabled.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await DetailAsync(token, id)).GetProperty("status").GetString().ShouldBe("Active");
    }

    [Fact]
    public async Task ListRoles_ShowsTheHolderCount_AndSortsByIt()
    {
        var token = await LoginAsOperatorAsync("list-admin", _manageRoles);
        var many = await Seed.RoleAsync("列表-人多", [], sortOrder: 91);
        var few = await Seed.RoleAsync("列表-人少", [], sortOrder: 90);
        foreach (var account in new[] { "list-a", "list-b" })
        {
            await Seed.GrantAsync(await Seed.UserAsync(account, Password), many);
        }

        await Seed.GrantAsync(await Seed.UserAsync("list-c", Password), few);

        var byCount = await JsonAsync(await SendAsync(HttpMethod.Get, "/roles?q=列表-&sort=-holderCount", token));
        var bySort = await JsonAsync(await SendAsync(HttpMethod.Get, "/roles?q=列表-", token));

        var rows = byCount.GetProperty("items").EnumerateArray().ToList();
        rows.Select(r => r.GetProperty("id").GetGuid()).ShouldBe([many, few]);
        rows.Select(r => r.GetProperty("holderCount").GetInt32()).ShouldBe([2, 1]);
        bySort.GetProperty("items").EnumerateArray().Select(r => r.GetProperty("id").GetGuid()).ShouldBe([few, many]);
    }

    [Fact]
    public async Task Catalogs_ListEveryModulesPermissionsAndScopeObjects()
    {
        var token = await LoginAsOperatorAsync("catalog-admin", [IdentityPermissions.RoleView]);

        var permissions = await JsonAsync(await SendAsync(HttpMethod.Get, "/permissions", token));
        var scopes = await JsonAsync(await SendAsync(HttpMethod.Get, "/data-scope-objects", token));

        var manage = permissions.EnumerateArray().Single(p => string.Equals(p.GetProperty("code").GetString(), IdentityPermissions.RoleManage, StringComparison.Ordinal));
        manage.GetProperty("module").GetString().ShouldBe("Identity");
        manage.GetProperty("sensitive").GetBoolean().ShouldBeTrue();
        permissions.EnumerateArray().Select(p => p.GetProperty("code").GetString()).Contains(SamplePermissions.View, StringComparer.Ordinal).ShouldBeTrue();
        scopes.EnumerateArray().Select(s => s.GetProperty("code").GetString()).Contains(SampleScopes.Specimen, StringComparer.Ordinal).ShouldBeTrue();
    }

    [Fact]
    public async Task GetRole_ThatDoesNotExist_IsNotFound()
    {
        var token = await LoginAsOperatorAsync("missing-admin", _manageRoles);

        var response = await SendAsync(HttpMethod.Get, $"/roles/{Guid.CreateVersion7()}", token);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Identity.RoleNotFound");
    }
}
