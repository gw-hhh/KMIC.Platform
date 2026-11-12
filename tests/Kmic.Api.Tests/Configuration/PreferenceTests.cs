using System.Net;
using System.Text.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Configuration.Application;
using Kmic.Identity.Application;
using Npgsql;
using Shouldly;

namespace Kmic.Api.Tests.Configuration;

/// <summary>
/// 个人偏好：默认值、校验、不允许个人设置的偏好、偏好族、模拟登录、表上的约束，端到端
/// </summary>
public sealed class PreferenceTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/configuration";
    private const string PageSize = "Sample.List.PageSize";
    private const string Footer = "Sample.Report.Footer";

    private static readonly int[] _notAnObject = [1, 2, 3];

    private Task<HttpResponseMessage> CallAsync(HttpMethod method, string path, string token, object? body = null) =>
        SendAsync(method, Root + path, token, body);

    private Task<HttpResponseMessage> PutMineAsync(string token, string key, object value) =>
        CallAsync(HttpMethod.Put, $"/preferences/me/{key}", token, new { value });

    private async Task<JsonElement[]> MineAsync(string token)
    {
        var response = await CallAsync(HttpMethod.Get, "/preferences/me", token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return [.. (await JsonAsync(response)).GetProperty("preferences").EnumerateArray()];
    }

    private static JsonElement? Find(JsonElement[] items, string key) =>
        items.Cast<JsonElement?>().SingleOrDefault(p => string.Equals(p!.Value.GetProperty("key").GetString(), key, StringComparison.Ordinal));

    private static async Task<string> CodeOfAsync(HttpResponseMessage response) => (await JsonAsync(response)).GetProperty("code").GetString()!;

    private async Task InsertRowAsync(string key, string layer, Guid? role, Guid? user)
    {
        await using var data = NpgsqlDataSource.Create(ConnectionString);
        await using var insert = data.CreateCommand("""
            INSERT INTO "Configuration"."PreferenceValue" ("Id", "Key", "Layer", "RoleId", "UserId", "Value", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            VALUES (@id, @key, @layer, @role, @user, '{"value": 30}', now(), '00000000-0000-0000-0000-000000000001', 0)
            """);
        insert.Parameters.AddWithValue("id", Guid.CreateVersion7());
        insert.Parameters.AddWithValue("key", key);
        insert.Parameters.AddWithValue("layer", layer);
        insert.Parameters.AddWithValue("role", (object?)role ?? DBNull.Value);
        insert.Parameters.AddWithValue("user", (object?)user ?? DBNull.Value);
        await insert.ExecuteNonQueryAsync(Ct);
    }

    [Fact]
    public async Task TheDeclaredPreferences_AreAllListed_WithTheirDefaultAndWhetherThePersonCanChangeThem()
    {
        var user = await LoginAsOperatorAsync("pref-mine-defaults", []);

        var items = await MineAsync(user);

        var pageSize = Find(items, PageSize).ShouldNotBeNull();
        pageSize.GetProperty("displayName").GetString().ShouldBe("标本列表每页条数");
        pageSize.GetProperty("value").GetProperty("value").GetInt32().ShouldBe(20);
        pageSize.GetProperty("source").GetString().ShouldBe("Default");
        pageSize.GetProperty("isFamily").GetBoolean().ShouldBeFalse();
        pageSize.GetProperty("canSet").GetBoolean().ShouldBeTrue();
        Find(items, Footer).ShouldNotBeNull().GetProperty("canSet").GetBoolean().ShouldBeFalse();
        items.Select(i => i.GetProperty("key").GetString()).ShouldBe(items.Select(i => i.GetProperty("key").GetString()).Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task AValueThatFailsThePreferencesOwnValidation_ComesBackFieldByField_AndNothingIsSaved()
    {
        var user = await LoginAsOperatorAsync("pref-mine-invalid", []);

        var outOfRange = await PutMineAsync(user, PageSize, new { value = 5 });
        var wrongShape = await PutMineAsync(user, PageSize, new { value = "很多" });
        var notAnObject = await CallAsync(HttpMethod.Put, $"/preferences/me/{PageSize}", user, new { value = 7 });

        outOfRange.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await JsonAsync(outOfRange);
        problem.GetProperty("code").GetString().ShouldBe("Configuration.PreferenceInvalid");
        problem.GetProperty("errors").EnumerateArray().Select(e => e.GetProperty("path").GetString()).ShouldBe(["value"]);
        wrongShape.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(wrongShape)).ShouldBe("Configuration.PreferenceInvalid");
        notAnObject.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        Find(await MineAsync(user), PageSize)!.Value.GetProperty("source").GetString().ShouldBe("Default");
    }

    [Fact]
    public async Task AKeyThatIsNeitherDeclaredNorInAFamily_IsNotFound()
    {
        var user = await LoginAsOperatorAsync("pref-mine-unknown", []);

        var undeclared = await PutMineAsync(user, "Sample.Nothing.Here", new { value = 1 });
        var badTableKey = await PutMineAsync(user, "Ui.Table.TooFewSegments", new { a = 1 });
        var reset = await CallAsync(HttpMethod.Post, "/preferences/me/Sample.Nothing.Here/reset", user);
        var tooLong = await PutMineAsync(user, "Ui.Table.Sample.Specimens." + new string('a', 130), new { a = 1 });

        undeclared.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeOfAsync(undeclared)).ShouldBe("Configuration.PreferenceNotFound");
        badTableKey.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        reset.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        tooLong.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task APreferenceThatDoesNotAllowThePersonalLayer_CanOnlyBeSetByAnAdministrator()
    {
        var admin = await LoginAsOperatorAsync("pref-footer-admin", [ConfigurationPermissions.PreferenceManage]);
        var user = await LoginAsOperatorAsync("pref-footer-user", []);

        var byUser = await PutMineAsync(user, Footer, new { text = "我想改页脚" });
        var bySystem = await CallAsync(HttpMethod.Put, $"/preferences/{Footer}/layers/system", admin, new { value = new { text = "统一页脚" }, expectedVersion = (long?)null });
        var tooLong = await CallAsync(HttpMethod.Put, $"/preferences/{Footer}/layers/system", admin, new { value = new { text = new string('长', 101) }, expectedVersion = (long?)0 });

        byUser.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(byUser)).ShouldBe("Configuration.PreferenceLayerNotAllowed");
        bySystem.StatusCode.ShouldBe(HttpStatusCode.OK);
        tooLong.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await JsonAsync(tooLong)).GetProperty("errors").EnumerateArray().Select(e => e.GetProperty("path").GetString()).ShouldBe(["text"]);
        var footer = Find(await MineAsync(user), Footer).ShouldNotBeNull();
        footer.GetProperty("value").GetProperty("text").GetString().ShouldBe("统一页脚");
        footer.GetProperty("source").GetString().ShouldBe("System");
    }

    [Fact]
    public async Task APreferenceFamilyKey_StoresAnyObject_ShowsUpOnlyOnceItHasAValue_AndCanBeRestored()
    {
        var user = await LoginAsOperatorAsync("pref-family-user", []);
        var table = "Ui.Table.Sample.Specimens.Main";
        (Find(await MineAsync(user), table) is null).ShouldBeTrue();

        var saved = await PutMineAsync(user, table, new { schemaVersion = 1, columns = new { number = new { visible = true, width = 120 } }, pageSize = 50 });
        var shell = await PutMineAsync(user, "Ui.Shell", new { schemaVersion = 1, theme = "dark" });

        saved.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        shell.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var items = await MineAsync(user);
        var tableItem = Find(items, table).ShouldNotBeNull();
        tableItem.GetProperty("isFamily").GetBoolean().ShouldBeTrue();
        tableItem.GetProperty("source").GetString().ShouldBe("UserRole");
        tableItem.GetProperty("canSet").GetBoolean().ShouldBeTrue();
        tableItem.GetProperty("value").GetProperty("columns").GetProperty("number").GetProperty("width").GetInt32().ShouldBe(120);
        Find(items, "Ui.Shell")!.Value.GetProperty("value").GetProperty("theme").GetString().ShouldBe("dark");

        (await PutMineAsync(user, table, new { schemaVersion = 1, pageSize = 20 })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        Find(await MineAsync(user), table)!.Value.GetProperty("value").GetProperty("pageSize").GetInt32().ShouldBe(20);
        (await CallAsync(HttpMethod.Post, $"/preferences/me/{table}/reset", user)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await CallAsync(HttpMethod.Post, $"/preferences/me/{table}/reset", user)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (Find(await MineAsync(user), table) is null).ShouldBeTrue();
    }

    [Fact]
    public async Task AFamilyValue_MustBeAnObjectNotLargerThanTheLimit()
    {
        var user = await LoginAsOperatorAsync("pref-family-limit", []);
        var table = "Ui.Table.Sample.Specimens.Wide";

        var notAnObject = await CallAsync(HttpMethod.Put, $"/preferences/me/{table}", user, new { value = _notAnObject });
        var oversize = await PutMineAsync(user, table, new { blob = new string('x', 17 * 1024) });
        var justUnder = await PutMineAsync(user, table, new { blob = new string('x', 15 * 1024) });

        notAnObject.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(notAnObject)).ShouldBe("Configuration.PreferenceInvalid");
        oversize.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        justUnder.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AReadOnlyImpersonationSession_SeesTheTargetsPreferences_ButCannotChangeThem()
    {
        var admin = await LoginAsOperatorAsync("pref-imp-admin", [IdentityPermissions.UserView, IdentityPermissions.UserImpersonate]);
        var targetRole = await Seed.RoleAsync("被模拟-pref", []);
        var target = await Seed.UserAsync("pref-imp-target", Password, displayName: "被模拟者");
        await Seed.GrantAsync(target, targetRole);
        var targetToken = (await LoginTokensAsync("pref-imp-target")).GetProperty("access_token").GetString()!;
        (await PutMineAsync(targetToken, PageSize, new { value = 33 })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var start = await SendAsync(HttpMethod.Post, $"/users/{target}/impersonation", admin, new { roleId = (Guid?)null, reason = "看他的列表为什么每页只有 33 条" });
        start.StatusCode.ShouldBe(HttpStatusCode.OK);
        var theirs = (await JsonAsync(start)).GetProperty("access_token").GetString()!;

        var read = await MineAsync(theirs);
        var write = await PutMineAsync(theirs, PageSize, new { value = 90 });

        Find(read, PageSize)!.Value.GetProperty("value").GetProperty("value").GetInt32().ShouldBe(33);
        write.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CodeOfAsync(write)).ShouldBe("Common.ImpersonationReadOnly");
        Find(await MineAsync(targetToken), PageSize)!.Value.GetProperty("value").GetProperty("value").GetInt32().ShouldBe(33);
    }

    [Fact]
    public async Task TheTable_RefusesTwoRowsForTheSameKeyAndLayer_AndRowsWhoseOwnerColumnsDoNotMatchTheirLayer()
    {
        var role = Guid.CreateVersion7();
        var user = Guid.CreateVersion7();
        await InsertRowAsync("Sample.Row.Check", "System", null, null);

        var duplicateSystem = await Should.ThrowAsync<PostgresException>(async () => await InsertRowAsync("Sample.Row.Check", "System", null, null));
        var systemWithRole = await Should.ThrowAsync<PostgresException>(async () => await InsertRowAsync("Sample.Row.Other", "System", role, null));
        var roleWithoutRole = await Should.ThrowAsync<PostgresException>(async () => await InsertRowAsync("Sample.Row.Other", "Role", null, null));
        var userWithoutUser = await Should.ThrowAsync<PostgresException>(async () => await InsertRowAsync("Sample.Row.Other", "UserRole", role, null));
        await InsertRowAsync("Sample.Row.Check", "Role", role, null);
        await InsertRowAsync("Sample.Row.Check", "UserRole", role, user);

        duplicateSystem.SqlState.ShouldBe("23505");
        systemWithRole.SqlState.ShouldBe("23514");
        roleWithoutRole.SqlState.ShouldBe("23514");
        userWithoutUser.SqlState.ShouldBe("23514");
    }
}
