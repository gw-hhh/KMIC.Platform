using System.Net;
using System.Text.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Configuration.Application;
using Kmic.Sample.Application;
using Shouldly;

namespace Kmic.Api.Tests.Configuration;

/// <summary>
/// 偏好的四层覆盖：默认、系统、角色、本人，逐层设置、逐层恢复、缓存失效，端到端
/// </summary>
/// <remarks>
/// 系统层是全局的，同一个类里的测试共用一个库，所以只有这一个类动 Sample.List.PageSize 的系统层
/// </remarks>
public sealed class PreferenceResolutionTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/configuration";
    private const string PageSize = "Sample.List.PageSize";

    private static readonly string[] _manage = [ConfigurationPermissions.PreferenceManage];

    private Task<HttpResponseMessage> CallAsync(HttpMethod method, string path, string token, object? body = null) =>
        SendAsync(method, Root + path, token, body);

    private async Task<string> UserInRoleAsync(string account, Guid role)
    {
        var user = await Seed.UserAsync(account, Password, displayName: "用户 " + account);
        await Seed.GrantAsync(user, role);
        return (await LoginTokensAsync(account)).GetProperty("access_token").GetString()!;
    }

    private async Task<(int Value, string Source)> PageSizeAsync(string token)
    {
        var response = await CallAsync(HttpMethod.Get, "/preferences/me", token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var item = (await JsonAsync(response)).GetProperty("preferences").EnumerateArray().Single(p => string.Equals(p.GetProperty("key").GetString(), PageSize, StringComparison.Ordinal));
        return (item.GetProperty("value").GetProperty("value").GetInt32(), item.GetProperty("source").GetString()!);
    }

    private async Task<HttpResponseMessage> SetLayerAsync(string admin, Guid? role, int value, long? expectedVersion = null, string key = PageSize) =>
        await CallAsync(HttpMethod.Put, role is null ? $"/preferences/{key}/layers/system" : $"/preferences/{key}/layers/roles/{role}", admin, new { value = new { value }, expectedVersion });

    private async Task<HttpResponseMessage> ResetLayerAsync(string admin, Guid? role, long expectedVersion, string key = PageSize) =>
        await CallAsync(HttpMethod.Post, role is null ? $"/preferences/{key}/layers/system/reset" : $"/preferences/{key}/layers/roles/{role}/reset", admin, new { expectedVersion });

    private async Task<JsonElement> DefinitionAsync(string admin, string key)
    {
        var response = await CallAsync(HttpMethod.Get, "/preferences", admin);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await JsonAsync(response)).GetProperty("preferences").EnumerateArray().Single(p => string.Equals(p.GetProperty("key").GetString(), key, StringComparison.Ordinal));
    }

    private static long VersionOf(JsonElement definition, Guid? role) =>
        role is null
            ? definition.GetProperty("systemLayer").GetProperty("version").GetInt64()
            : definition.GetProperty("roleLayers").EnumerateArray().Single(l => l.GetProperty("roleId").GetGuid() == role).GetProperty("version").GetInt64();

    private static async Task<string> CodeOfAsync(HttpResponseMessage response) => (await JsonAsync(response)).GetProperty("code").GetString()!;

    [Fact]
    public async Task TheValueComesFromTheNearestLayer_AndRestoringFallsThroughTheLayersOneByOne()
    {
        var admin = await LoginAsOperatorAsync("pref-res-admin", _manage);
        var role = await Seed.RoleAsync("偏好角色-res", []);
        var alice = await UserInRoleAsync("pref-res-alice", role);
        var bob = await UserInRoleAsync("pref-res-bob", role);
        var outsider = await UserInRoleAsync("pref-res-outsider", await Seed.RoleAsync("另一个角色-res", []));
        (await PageSizeAsync(alice)).ShouldBe((20, "Default"));

        (await SetLayerAsync(admin, null, 50)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await PageSizeAsync(alice)).ShouldBe((50, "System"));

        (await SetLayerAsync(admin, role, 100)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await PageSizeAsync(alice)).ShouldBe((100, "Role"));
        (await PageSizeAsync(bob)).ShouldBe((100, "Role"));
        (await PageSizeAsync(outsider)).ShouldBe((50, "System"));

        (await CallAsync(HttpMethod.Put, $"/preferences/me/{PageSize}", alice, new { value = new { value = 30 } })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await PageSizeAsync(alice)).ShouldBe((30, "UserRole"));
        (await PageSizeAsync(bob)).ShouldBe((100, "Role"));

        (await CallAsync(HttpMethod.Post, $"/preferences/me/{PageSize}/reset", alice)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await PageSizeAsync(alice)).ShouldBe((100, "Role"));

        var definition = await DefinitionAsync(admin, PageSize);
        (await ResetLayerAsync(admin, role, VersionOf(definition, role))).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await PageSizeAsync(alice)).ShouldBe((50, "System"));

        (await ResetLayerAsync(admin, null, VersionOf(definition, null))).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await PageSizeAsync(alice)).ShouldBe((20, "Default"));
    }

    [Fact]
    public async Task ThePreferencesReadByCode_FollowTheCurrentUserAndRole()
    {
        var admin = await LoginAsOperatorAsync("pref-code-admin", _manage);
        var role = await Seed.RoleAsync("偏好角色-code", []);
        var userId = await Seed.UserAsync("pref-code-user", Password, displayName: "读偏好的用户");
        await Seed.GrantAsync(userId, role);
        var token = (await LoginTokensAsync("pref-code-user")).GetProperty("access_token").GetString()!;
        var identity = TestIdentity.User("读偏好的用户") with { UserId = UserId.From(userId), RoleId = RoleId.From(role) };

        async Task<int> ReadAsync(TestIdentity? who)
        {
            await using var scope = who is null
                ? Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope()
                : Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateScope(who);
            return (await scope.ServiceProvider.GetRequiredService<IPreferences>().GetAsync<SampleListPageSize>(Ct)).Value;
        }

        (await ReadAsync(identity)).ShouldBe(20);
        (await SetLayerAsync(admin, null, 40, key: PageSize)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await SetLayerAsync(admin, role, 90)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await CallAsync(HttpMethod.Put, $"/preferences/me/{PageSize}", token, new { value = new { value = 25 } })).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await ReadAsync(identity)).ShouldBe(25);
        (await ReadAsync(identity with { UserId = UserId.New() })).ShouldBe(90);
        (await ReadAsync(identity with { RoleId = RoleId.New() })).ShouldBe(40);
        (await ReadAsync(null)).ShouldBe(40);

        // 系统层是全局的：用完恢复，别的测试才看得到默认值
        var definition = await DefinitionAsync(admin, PageSize);
        (await ResetLayerAsync(admin, role, VersionOf(definition, role))).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ResetLayerAsync(admin, null, VersionOf(definition, null))).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadAsync(null)).ShouldBe(20);
    }

    [Fact]
    public async Task ChangingALayer_NeedsTheManagePermission_AnExistingRole_AndTheVersionTheCallerSaw()
    {
        var admin = await LoginAsOperatorAsync("pref-guard-admin", _manage);
        var plain = await LoginAsOperatorAsync("pref-guard-plain", []);
        var role = await Seed.RoleAsync("偏好角色-guard", []);
        var key = "Ui.Table.Sample.Guard.Main";
        (await CallAsync(HttpMethod.Put, $"/preferences/{key}/layers/roles/{role}", admin, new { value = new { text = "第一版" }, expectedVersion = (long?)null })).StatusCode.ShouldBe(HttpStatusCode.OK);
        var version = VersionOf(await DefinitionAsync(admin, key), role);

        var byPlain = await SetLayerAsync(plain, role, 60, key: PageSize);
        var listByPlain = await CallAsync(HttpMethod.Get, "/preferences", plain);
        var noRole = await SetLayerAsync(admin, Guid.CreateVersion7(), 60);
        var thinksItIsEmpty = await CallAsync(HttpMethod.Put, $"/preferences/{key}/layers/roles/{role}", admin, new { value = new { text = "第二版" }, expectedVersion = (long?)null });
        var staleReset = await CallAsync(HttpMethod.Post, $"/preferences/{key}/layers/roles/{role}/reset", admin, new { expectedVersion = version + 7 });
        var updated = await CallAsync(HttpMethod.Put, $"/preferences/{key}/layers/roles/{role}", admin, new { value = new { text = "第二版" }, expectedVersion = version });

        byPlain.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        listByPlain.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        noRole.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeOfAsync(noRole)).ShouldBe("Configuration.PreferenceRoleNotFound");
        thinksItIsEmpty.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        staleReset.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        updated.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await DefinitionAsync(admin, key)).GetProperty("roleLayers")[0].GetProperty("value").GetProperty("text").GetString().ShouldBe("第二版");
    }

    [Fact]
    public async Task TheDefinitionsList_ShowsTheAllowedLayersTheDefaultAndTheCurrentLayerValues()
    {
        var admin = await LoginAsOperatorAsync("pref-list-admin", _manage);
        var role = await Seed.RoleAsync("偏好角色-list", []);
        var footer = "Sample.Report.Footer";
        var table = "Ui.Table.Sample.Specimens.Main";
        (await CallAsync(HttpMethod.Put, $"/preferences/{footer}/layers/system", admin, new { value = new { text = "系统页脚" }, expectedVersion = (long?)null })).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await CallAsync(HttpMethod.Put, $"/preferences/{table}/layers/roles/{role}", admin, new { value = new { schemaVersion = 1, pageSize = 50 }, expectedVersion = (long?)null })).StatusCode.ShouldBe(HttpStatusCode.OK);

        var footerItem = await DefinitionAsync(admin, footer);
        var pageSizeItem = await DefinitionAsync(admin, PageSize);
        var tableItem = await DefinitionAsync(admin, table);

        footerItem.GetProperty("module").GetString().ShouldBe("Sample");
        footerItem.GetProperty("isFamily").GetBoolean().ShouldBeFalse();
        footerItem.GetProperty("layers").EnumerateArray().Select(l => l.GetString()).ShouldBe(["Role", "System"]);
        footerItem.GetProperty("systemLayer").GetProperty("value").GetProperty("text").GetString().ShouldBe("系统页脚");
        footerItem.GetProperty("roleLayers").GetArrayLength().ShouldBe(0);
        pageSizeItem.GetProperty("layers").EnumerateArray().Select(l => l.GetString()).ShouldBe(["UserRole", "Role", "System"]);
        pageSizeItem.GetProperty("defaultValue").GetProperty("value").GetInt32().ShouldBe(20);
        tableItem.GetProperty("isFamily").GetBoolean().ShouldBeTrue();
        tableItem.GetProperty("module").ValueKind.ShouldBe(JsonValueKind.Null);
        tableItem.GetProperty("defaultValue").ValueKind.ShouldBe(JsonValueKind.Null);
        var roleLayer = tableItem.GetProperty("roleLayers").EnumerateArray().Single();
        roleLayer.GetProperty("roleName").GetString().ShouldBe("偏好角色-list");
        roleLayer.GetProperty("value").GetProperty("pageSize").GetInt32().ShouldBe(50);
    }
}
