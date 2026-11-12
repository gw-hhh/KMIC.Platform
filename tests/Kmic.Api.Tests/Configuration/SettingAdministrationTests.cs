using System.Net;
using System.Text.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Configuration.Application;
using Kmic.Sample.Application;
using Npgsql;
using Shouldly;

namespace Kmic.Api.Tests.Configuration;

/// <summary>
/// 系统设置：列表、修改、校验、恢复默认、变更历史、清除已不存在的设置，端到端
/// </summary>
public sealed class SettingAdministrationTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/configuration";
    private const string Key = "Sample.Submission";

    private static readonly string[] _editor = [ConfigurationPermissions.SettingView, SamplePermissions.SettingManage];

    private Task<HttpResponseMessage> CallAsync(HttpMethod method, string path, string token, object? body = null) =>
        SendAsync(method, Root + path, token, body);

    private async Task<JsonElement> ItemAsync(string token)
    {
        var response = await CallAsync(HttpMethod.Get, $"/settings/{Key}", token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await JsonAsync(response);
    }

    private async Task<HttpResponseMessage> SetAsync(string token, object value, string reason = "按新流程调整", long? expectedVersion = null) =>
        await CallAsync(HttpMethod.Put, $"/settings/{Key}", token, new { value, reason, expectedVersion });

    private async Task<SampleSubmissionSetting> EffectiveAsync()
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope();
        return await scope.ServiceProvider.GetRequiredService<ISettings>().GetAsync<SampleSubmissionSetting>(Ct);
    }

    private async Task ResetToDefaultAsync(string token)
    {
        var item = await ItemAsync(token);
        if (item.GetProperty("isModified").GetBoolean())
        {
            (await CallAsync(HttpMethod.Post, $"/settings/{Key}/reset", token, new { reason = "测试开始前恢复默认", expectedVersion = item.GetProperty("version").GetInt64() })).StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    private static async Task<string> CodeOfAsync(HttpResponseMessage response) => (await JsonAsync(response)).GetProperty("code").GetString()!;

    [Fact]
    public async Task TheList_ShowsTheFieldsTheDefaultAndWhatTheCurrentUserMayDo()
    {
        var editor = await LoginAsOperatorAsync("set-list-editor", _editor);
        var viewer = await LoginAsOperatorAsync("set-list-viewer", [ConfigurationPermissions.SettingView]);

        var asEditor = await JsonAsync(await CallAsync(HttpMethod.Get, "/settings", editor));
        var asViewer = await JsonAsync(await CallAsync(HttpMethod.Get, "/settings", viewer));

        var setting = asEditor.GetProperty("settings").EnumerateArray().Single(s => string.Equals(s.GetProperty("key").GetString(), Key, StringComparison.Ordinal));
        setting.GetProperty("displayName").GetString().ShouldBe("标本提交");
        setting.GetProperty("module").GetString().ShouldBe("Sample");
        setting.GetProperty("group").GetString().ShouldBe("示例");
        setting.GetProperty("editPermission").GetString().ShouldBe(SamplePermissions.SettingManage);
        var maxItems = setting.GetProperty("fields").EnumerateArray().Single(f => string.Equals(f.GetProperty("name").GetString(), "maxItems", StringComparison.Ordinal));
        maxItems.GetProperty("type").GetString().ShouldBe("int");
        maxItems.GetProperty("displayName").GetString().ShouldBe("明细上限");
        maxItems.GetProperty("unit").GetString().ShouldBe("个");
        setting.GetProperty("fields").EnumerateArray().Single(f => string.Equals(f.GetProperty("name").GetString(), "contactEmail", StringComparison.Ordinal)).GetProperty("isNullable").GetBoolean().ShouldBeTrue();
        setting.GetProperty("defaultValue").GetProperty("maxItems").GetInt32().ShouldBe(50);
        string.Join(",", setting.GetProperty("allowedActions").EnumerateArray().Select(a => a.GetString())).ShouldBe("Edit,Reset");
        asViewer.GetProperty("settings").EnumerateArray().Single(s => string.Equals(s.GetProperty("key").GetString(), Key, StringComparison.Ordinal)).GetProperty("allowedActions").GetArrayLength().ShouldBe(0);
    }

    [Fact]
    public async Task Setting_ChangesTheEffectiveValue_ForTheCodeThatReadsIt_AndResetBringsTheDefaultBack()
    {
        var editor = await LoginAsOperatorAsync("set-edit-editor", _editor);
        await ResetToDefaultAsync(editor);
        (await EffectiveAsync()).MaxItems.ShouldBe(50);

        var set = await SetAsync(editor, new { maxItems = 120, reasonRequired = true, contactEmail = "ops@example.com" });

        set.StatusCode.ShouldBe(HttpStatusCode.OK);
        var effective = await EffectiveAsync();
        effective.ShouldBe(new SampleSubmissionSetting(120, true, "ops@example.com"));
        var item = await ItemAsync(editor);
        item.GetProperty("isModified").GetBoolean().ShouldBeTrue();
        item.GetProperty("currentValue").GetProperty("maxItems").GetInt32().ShouldBe(120);

        var reset = await CallAsync(HttpMethod.Post, $"/settings/{Key}/reset", editor, new { reason = "恢复默认", expectedVersion = item.GetProperty("version").GetInt64() });

        reset.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await EffectiveAsync()).ShouldBe(SampleSubmissionSetting.Default);
        (await ItemAsync(editor)).GetProperty("isModified").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task AValueThatFailsTheSettingsOwnValidation_ComesBackFieldByField_AndNothingChanges()
    {
        var editor = await LoginAsOperatorAsync("set-invalid-editor", _editor);
        await ResetToDefaultAsync(editor);

        var invalid = await SetAsync(editor, new { maxItems = 0, reasonRequired = false, contactEmail = "不是邮箱" });
        var wrongShape = await SetAsync(editor, new { maxItems = "很多", reasonRequired = false });
        var notAnObject = await CallAsync(HttpMethod.Put, $"/settings/{Key}", editor, new { value = 5, reason = "不是对象", expectedVersion = (long?)null });

        invalid.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await JsonAsync(invalid);
        problem.GetProperty("code").GetString().ShouldBe("Configuration.SettingInvalid");
        problem.GetProperty("errors").EnumerateArray().Select(e => e.GetProperty("path").GetString()).Order(StringComparer.Ordinal).ShouldBe(["contactEmail", "maxItems"]);
        wrongShape.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(wrongShape)).ShouldBe("Configuration.SettingInvalid");
        notAnObject.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ItemAsync(editor)).GetProperty("isModified").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task ChangingASetting_NeedsTheViewPermissionAndTheSettingsOwnEditPermission()
    {
        var editor = await LoginAsOperatorAsync("set-perm-editor", _editor);
        var viewer = await LoginAsOperatorAsync("set-perm-viewer", [ConfigurationPermissions.SettingView]);
        var writerOnly = await LoginAsOperatorAsync("set-perm-writer", [SamplePermissions.SettingManage]);
        await ResetToDefaultAsync(editor);

        var byViewer = await SetAsync(viewer, new { maxItems = 10 });
        var byOnlyEdit = await SetAsync(writerOnly, new { maxItems = 10 });
        var listByOnlyEdit = await CallAsync(HttpMethod.Get, "/settings", writerOnly);
        var unknown = await CallAsync(HttpMethod.Put, "/settings/Sample.Nothing", editor, new { value = new { }, reason = "不存在的设置", expectedVersion = (long?)null });
        var noReason = await SetAsync(editor, new { maxItems = 10 }, reason: " ");

        byViewer.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        byOnlyEdit.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        listByOnlyEdit.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeOfAsync(unknown)).ShouldBe("Configuration.SettingNotFound");
        noReason.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ASecondEditorWhoSawAnOlderVersion_GetsAConflict()
    {
        var editor = await LoginAsOperatorAsync("set-conflict-editor", _editor);
        await ResetToDefaultAsync(editor);
        (await SetAsync(editor, new { maxItems = 60 })).StatusCode.ShouldBe(HttpStatusCode.OK);
        var version = (await ItemAsync(editor)).GetProperty("version").GetInt64();
        (await SetAsync(editor, new { maxItems = 70 }, expectedVersion: version)).StatusCode.ShouldBe(HttpStatusCode.OK);

        var stale = await SetAsync(editor, new { maxItems = 80 }, expectedVersion: version);
        var thinksItIsDefault = await SetAsync(editor, new { maxItems = 90 });

        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(stale)).ShouldBe("Common.VersionConflict");
        thinksItIsDefault.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await EffectiveAsync()).MaxItems.ShouldBe(70);
    }

    [Fact]
    public async Task TheChangeHistory_RecordsWhoChangedWhatAndWhy_AndCannotBeRewritten()
    {
        var editor = await LoginAsOperatorAsync("set-history-editor", _editor);
        await ResetToDefaultAsync(editor);
        (await SetAsync(editor, new { maxItems = 33 }, reason: "第一次调整")).StatusCode.ShouldBe(HttpStatusCode.OK);
        var version = (await ItemAsync(editor)).GetProperty("version").GetInt64();
        (await SetAsync(editor, new { maxItems = 44 }, reason: "第二次调整", expectedVersion: version)).StatusCode.ShouldBe(HttpStatusCode.OK);

        var page = await JsonAsync(await CallAsync(HttpMethod.Get, $"/setting-changes?key={Key}&pageSize=2", editor));

        var latest = page.GetProperty("items")[0];
        latest.GetProperty("key").GetString().ShouldBe(Key);
        latest.GetProperty("reason").GetString().ShouldBe("第二次调整");
        latest.GetProperty("oldValue").GetProperty("maxItems").GetInt32().ShouldBe(33);
        latest.GetProperty("newValue").GetProperty("maxItems").GetInt32().ShouldBe(44);
        latest.GetProperty("changedByName").GetString().ShouldBe("操作者 set-history-editor");
        latest.GetProperty("traceId").GetString().ShouldNotBeNullOrWhiteSpace();
        page.GetProperty("items")[1].GetProperty("reason").GetString().ShouldBe("第一次调整");

        await using var data = NpgsqlDataSource.Create(ConnectionString);
        await using var rewrite = data.CreateCommand("""UPDATE "Configuration"."SystemSettingChange" SET "Reason" = '改写' WHERE true""");
        var update = await Should.ThrowAsync<PostgresException>(async () => await rewrite.ExecuteNonQueryAsync(Ct));
        update.SqlState.ShouldBe("42501");
        await using var erase = data.CreateCommand("""DELETE FROM "Configuration"."SystemSettingChange" WHERE true""");
        var delete = await Should.ThrowAsync<PostgresException>(async () => await erase.ExecuteNonQueryAsync(Ct));
        delete.SqlState.ShouldBe("42501");
    }

    [Fact]
    public async Task ASettingRemovedFromTheCode_LeavesItsValue_AndOnlyThePurgePermissionCanClearIt()
    {
        var editor = await LoginAsOperatorAsync("set-purge-editor", _editor);
        var purger = await LoginAsOperatorAsync("set-purge-purger", [ConfigurationPermissions.SettingView, ConfigurationPermissions.SettingManage]);
        await using (var data = NpgsqlDataSource.Create(ConnectionString))
        {
            await using var insert = data.CreateCommand("""
                INSERT INTO "Configuration"."SystemSettingValue" ("Id", "Key", "Value", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
                VALUES (@id, 'Sample.Gone', '{"old": true}', now(), '00000000-0000-0000-0000-000000000001', 0)
                """);
            insert.Parameters.AddWithValue("id", Guid.CreateVersion7());
            await insert.ExecuteNonQueryAsync(Ct);
        }

        var listed = await JsonAsync(await CallAsync(HttpMethod.Get, "/settings", editor));
        var obsolete = listed.GetProperty("obsolete").EnumerateArray().Single(o => string.Equals(o.GetProperty("key").GetString(), "Sample.Gone", StringComparison.Ordinal));
        var version = obsolete.GetProperty("version").GetInt64();

        var denied = await CallAsync(HttpMethod.Post, "/settings/Sample.Gone/purge", editor, new { reason = "清理", expectedVersion = version });
        var stillDeclared = await CallAsync(HttpMethod.Post, $"/settings/{Key}/purge", purger, new { reason = "不该成功", expectedVersion = 0L });
        var purged = await CallAsync(HttpMethod.Post, "/settings/Sample.Gone/purge", purger, new { reason = "代码里早删掉了", expectedVersion = version });

        denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        stillDeclared.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(stillDeclared)).ShouldBe("Configuration.InvalidState");
        purged.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(await CallAsync(HttpMethod.Get, "/settings", editor))).GetProperty("obsolete").EnumerateArray().Any(o => string.Equals(o.GetProperty("key").GetString(), "Sample.Gone", StringComparison.Ordinal)).ShouldBeFalse();
        var history = await JsonAsync(await CallAsync(HttpMethod.Get, "/setting-changes?key=Sample.Gone", purger));
        history.GetProperty("items")[0].GetProperty("newValue").ValueKind.ShouldBe(JsonValueKind.Null);
    }
}
