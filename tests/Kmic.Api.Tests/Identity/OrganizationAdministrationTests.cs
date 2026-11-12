using System.Net;
using System.Text.Json;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Identity.Application;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 组织单元管理与用户归属：整棵树一个聚合，版本号是树的版本号，端到端
/// </summary>
public sealed class OrganizationAdministrationTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private static readonly string[] _manage = [IdentityPermissions.OrgUnitManage, IdentityPermissions.UserView, IdentityPermissions.UserManage];

    private async Task<JsonElement> TreeAsync(string token)
    {
        var response = await SendAsync(HttpMethod.Get, "/org-tree", token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await JsonAsync(response);
    }

    private async Task<long> TreeVersionAsync(string token) => (await TreeAsync(token)).GetProperty("version").GetInt64();

    private async Task<Guid> AddUnitAsync(string token, Guid parent, string name, string? code = null)
    {
        var response = await SendAsync(HttpMethod.Post, "/org-units", token, new { expectedVersion = await TreeVersionAsync(token), parentId = parent, name, code });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await JsonAsync(response)).GetProperty("id").GetGuid();
    }

    private static string[] ChildrenOf(JsonElement tree, Guid parent) =>
        [.. tree.GetProperty("units").EnumerateArray().Where(u => u.GetProperty("parentId").ValueKind == JsonValueKind.String && u.GetProperty("parentId").GetGuid() == parent).Select(u => u.GetProperty("name").GetString()!)];

    private static JsonElement UnitOf(JsonElement tree, Guid id) =>
        tree.GetProperty("units").EnumerateArray().Single(u => u.GetProperty("id").GetGuid() == id);

    private async Task<HttpResponseMessage> PostUnitActionAsync(string token, Guid id, string action) =>
        await SendAsync(HttpMethod.Post, $"/org-units/{id}/{action}", token, new { expectedVersion = await TreeVersionAsync(token) });

    private static async Task<string> CodeOfAsync(HttpResponseMessage response) => (await JsonAsync(response)).GetProperty("code").GetString()!;

    [Fact]
    public async Task TheTree_CanBeReadByAnyoneWhoIsLoggedIn_AndShowsTheRoot()
    {
        var root = await Seed.OrgTreeAsync();
        var nobody = await LoginAsOperatorAsync("org-nobody", []);

        var tree = await TreeAsync(nobody);

        var rootUnit = UnitOf(tree, root);
        rootUnit.GetProperty("name").GetString().ShouldBe("总部");
        rootUnit.GetProperty("parentId").ValueKind.ShouldBe(JsonValueKind.Null);
        rootUnit.GetProperty("status").GetString().ShouldBe("Active");
    }

    [Fact]
    public async Task AddingUnits_NeedsThePermission_AndReturnsTheNewTreeVersion()
    {
        var root = await Seed.OrgTreeAsync();
        var admin = await LoginAsOperatorAsync("org-add-admin", _manage);
        var nobody = await LoginAsOperatorAsync("org-add-nobody", [IdentityPermissions.UserView]);
        var before = await TreeVersionAsync(admin);

        var denied = await SendAsync(HttpMethod.Post, "/org-units", nobody, new { expectedVersion = before, parentId = root, name = "不该成功", code = (string?)null });
        var created = await SendAsync(HttpMethod.Post, "/org-units", admin, new { expectedVersion = before, parentId = root, name = "华东大区", code = "EAST" });

        denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await JsonAsync(created);
        body.GetProperty("version").GetInt64().ShouldBeGreaterThan(before);
        var tree = await TreeAsync(admin);
        tree.GetProperty("version").GetInt64().ShouldBe(body.GetProperty("version").GetInt64());
        var unit = UnitOf(tree, body.GetProperty("id").GetGuid());
        unit.GetProperty("code").GetString().ShouldBe("EAST");
        unit.GetProperty("parentId").GetGuid().ShouldBe(root);
    }

    [Fact]
    public async Task AddingUnits_WithASiblingNameInAnotherCase_OrAStaleVersion_IsAConflict()
    {
        var root = await Seed.OrgTreeAsync();
        var admin = await LoginAsOperatorAsync("org-dup-admin", _manage);
        await AddUnitAsync(admin, root, "销售部");
        var stale = await TreeVersionAsync(admin) - 1;

        var duplicate = await SendAsync(HttpMethod.Post, "/org-units", admin, new { expectedVersion = await TreeVersionAsync(admin), parentId = root, name = "销售部", code = (string?)null });
        var old = await SendAsync(HttpMethod.Post, "/org-units", admin, new { expectedVersion = stale, parentId = root, name = "过期的请求", code = (string?)null });

        duplicate.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(duplicate)).ShouldBe("Identity.OrgUnitNameExists");
        old.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(old)).ShouldBe("Common.VersionConflict");
    }

    [Fact]
    public async Task RenamingAUnit_ChangesItsNameAndCode()
    {
        var root = await Seed.OrgTreeAsync();
        var admin = await LoginAsOperatorAsync("org-rename-admin", _manage);
        var unit = await AddUnitAsync(admin, root, "改名前");

        var renamed = await SendAsync(HttpMethod.Put, $"/org-units/{unit}", admin, new { expectedVersion = await TreeVersionAsync(admin), name = "改名后", code = "NEW" });

        renamed.StatusCode.ShouldBe(HttpStatusCode.OK);
        var item = UnitOf(await TreeAsync(admin), unit);
        item.GetProperty("name").GetString().ShouldBe("改名后");
        item.GetProperty("code").GetString().ShouldBe("NEW");
    }

    [Fact]
    public async Task Moving_CannotCreateACycle_AndTheRootCannotMove()
    {
        var root = await Seed.OrgTreeAsync();
        var admin = await LoginAsOperatorAsync("org-move-admin", _manage);
        var parent = await AddUnitAsync(admin, root, "移动-上级");
        var child = await AddUnitAsync(admin, parent, "移动-下级");
        var elsewhere = await AddUnitAsync(admin, root, "移动-别处");

        var cycle = await SendAsync(HttpMethod.Post, $"/org-units/{parent}/move", admin, new { expectedVersion = await TreeVersionAsync(admin), newParentId = child });
        var rootMove = await SendAsync(HttpMethod.Post, $"/org-units/{root}/move", admin, new { expectedVersion = await TreeVersionAsync(admin), newParentId = elsewhere });
        var moved = await SendAsync(HttpMethod.Post, $"/org-units/{parent}/move", admin, new { expectedVersion = await TreeVersionAsync(admin), newParentId = elsewhere });

        cycle.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(cycle)).ShouldBe("Identity.OrgUnitCycle");
        (await CodeOfAsync(rootMove)).ShouldBe("Identity.OrgRootImmutable");
        moved.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tree = await TreeAsync(admin);
        ChildrenOf(tree, elsewhere).ShouldBe(["移动-上级"]);
        ChildrenOf(tree, parent).ShouldBe(["移动-下级"]);
    }

    [Fact]
    public async Task Reordering_NeedsTheCompleteListOfChildren()
    {
        var root = await Seed.OrgTreeAsync();
        var admin = await LoginAsOperatorAsync("org-order-admin", _manage);
        var parent = await AddUnitAsync(admin, root, "排序-上级");
        var first = await AddUnitAsync(admin, parent, "排序-一");
        var second = await AddUnitAsync(admin, parent, "排序-二");
        var third = await AddUnitAsync(admin, parent, "排序-三");

        var incomplete = await SendAsync(HttpMethod.Put, $"/org-units/{parent}/children-order", admin, new { expectedVersion = await TreeVersionAsync(admin), orderedChildren = new[] { first, second } });
        var reordered = await SendAsync(HttpMethod.Put, $"/org-units/{parent}/children-order", admin, new { expectedVersion = await TreeVersionAsync(admin), orderedChildren = new[] { third, first, second } });

        (await CodeOfAsync(incomplete)).ShouldBe("Identity.OrgUnitOrderInvalid");
        reordered.StatusCode.ShouldBe(HttpStatusCode.OK);
        ChildrenOf(await TreeAsync(admin), parent).ShouldBe(["排序-三", "排序-一", "排序-二"]);
    }

    [Fact]
    public async Task Disabling_IsRefusedWhileThereAreEnabledChildrenOrUsers_AndEnableBringsItBack()
    {
        var root = await Seed.OrgTreeAsync();
        var admin = await LoginAsOperatorAsync("org-disable-admin", _manage);
        var parent = await AddUnitAsync(admin, root, "停用-上级");
        var child = await AddUnitAsync(admin, parent, "停用-下级");
        var member = await Seed.UserAsync("org.disable.member", Password, displayName: "停用测试成员");
        var memberVersion = (await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{member}", admin))).GetProperty("version").GetInt64();
        (await SendAsync(HttpMethod.Put, $"/users/{member}/org-unit", admin, new { expectedVersion = memberVersion, orgUnitId = child })).StatusCode.ShouldBe(HttpStatusCode.OK);

        var withChild = await PostUnitActionAsync(admin, parent, "disable");
        var withUser = await PostUnitActionAsync(admin, child, "disable");
        var rootDisable = await PostUnitActionAsync(admin, root, "disable");

        withChild.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(withChild)).ShouldBe("Identity.OrgUnitInUse");
        withUser.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(withUser)).ShouldBe("Identity.OrgUnitInUse");
        (await CodeOfAsync(rootDisable)).ShouldBe("Identity.OrgRootImmutable");

        memberVersion = (await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{member}", admin))).GetProperty("version").GetInt64();
        (await SendAsync(HttpMethod.Put, $"/users/{member}/org-unit", admin, new { expectedVersion = memberVersion, orgUnitId = (Guid?)null })).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await PostUnitActionAsync(admin, child, "disable")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await PostUnitActionAsync(admin, parent, "disable")).StatusCode.ShouldBe(HttpStatusCode.OK);
        UnitOf(await TreeAsync(admin), parent).GetProperty("status").GetString().ShouldBe("Disabled");

        var childFirst = await PostUnitActionAsync(admin, child, "enable");
        var parentFirst = await PostUnitActionAsync(admin, parent, "enable");

        childFirst.StatusCode.ShouldNotBe(HttpStatusCode.OK);
        parentFirst.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await PostUnitActionAsync(admin, child, "enable")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AssigningAUser_ShowsTheUnitName_InTheDetailTheListAndTheUserOptions()
    {
        var root = await Seed.OrgTreeAsync();
        var admin = await LoginAsOperatorAsync("org-assign-admin", _manage);
        var unit = await AddUnitAsync(admin, root, "归属-华南大区");
        var member = await Seed.UserAsync("org.assign.member", Password, displayName: "归属测试成员");
        var version = (await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{member}", admin))).GetProperty("version").GetInt64();

        var assigned = await SendAsync(HttpMethod.Put, $"/users/{member}/org-unit", admin, new { expectedVersion = version, orgUnitId = unit });

        assigned.StatusCode.ShouldBe(HttpStatusCode.OK);
        var detail = await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{member}", admin));
        detail.GetProperty("orgUnitId").GetGuid().ShouldBe(unit);
        detail.GetProperty("orgUnitName").GetString().ShouldBe("归属-华南大区");
        var listed = (await JsonAsync(await SendAsync(HttpMethod.Get, $"/users?q=归属测试&orgUnitId={unit}", admin))).GetProperty("items").EnumerateArray().ShouldHaveSingleItem();
        listed.GetProperty("orgUnitName").GetString().ShouldBe("归属-华南大区");
        var option = (await JsonAsync(await SendAsync(HttpMethod.Get, "/user-options?q=归属测试", admin))).GetProperty("items").EnumerateArray().ShouldHaveSingleItem();
        option.GetProperty("orgUnitName").GetString().ShouldBe("归属-华南大区");
    }

    [Fact]
    public async Task AssigningAUser_ToADisabledOrUnknownUnit_IsRefused()
    {
        var root = await Seed.OrgTreeAsync();
        var admin = await LoginAsOperatorAsync("org-bad-admin", _manage);
        var closed = await AddUnitAsync(admin, root, "归属-已撤销");
        (await PostUnitActionAsync(admin, closed, "disable")).StatusCode.ShouldBe(HttpStatusCode.OK);
        var member = await Seed.UserAsync("org.bad.member", Password, displayName: "归属失败成员");
        var version = (await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{member}", admin))).GetProperty("version").GetInt64();

        var disabled = await SendAsync(HttpMethod.Put, $"/users/{member}/org-unit", admin, new { expectedVersion = version, orgUnitId = closed });
        var unknown = await SendAsync(HttpMethod.Put, $"/users/{member}/org-unit", admin, new { expectedVersion = version, orgUnitId = Guid.CreateVersion7() });

        disabled.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(disabled)).ShouldBe("Identity.OrgUnitDisabled");
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeOfAsync(unknown)).ShouldBe("Identity.OrgUnitNotFound");
    }

    [Fact]
    public async Task AssigningAUser_WhileTheUnitIsBeingDisabled_NeverLeavesAnEnabledUserInADisabledUnit()
    {
        var root = await Seed.OrgTreeAsync();
        var admin = await LoginAsOperatorAsync("org-race-admin", _manage);

        for (var round = 0; round < 5; round++)
        {
            var unit = await AddUnitAsync(admin, root, $"竞争-{round}");
            var member = await Seed.UserAsync($"org.race.member{round}", Password, displayName: $"竞争成员 {round}");
            var memberVersion = (await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{member}", admin))).GetProperty("version").GetInt64();
            var treeVersion = await TreeVersionAsync(admin);

            var assign = SendAsync(HttpMethod.Put, $"/users/{member}/org-unit", admin, new { expectedVersion = memberVersion, orgUnitId = unit });
            var disable = SendAsync(HttpMethod.Post, $"/org-units/{unit}/disable", admin, new { expectedVersion = treeVersion });
            var results = await Task.WhenAll(assign, disable);

            var tree = await TreeAsync(admin);
            var unitDisabled = string.Equals(UnitOf(tree, unit).GetProperty("status").GetString(), "Disabled", StringComparison.Ordinal);
            var assignedTo = (await JsonAsync(await SendAsync(HttpMethod.Get, $"/users/{member}", admin))).GetProperty("orgUnitId");
            var inside = assignedTo.ValueKind == JsonValueKind.String && assignedTo.GetGuid() == unit;
            (unitDisabled && inside).ShouldBeFalse($"第 {round} 轮：用户 {results[0].StatusCode}，停用 {results[1].StatusCode}");
        }
    }
}
