using System.Net;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Configuration.Application;
using Kmic.Configuration.Application.Dictionaries.Sync;
using Kmic.Configuration.Contracts;
using Kmic.Sample.Application;
using Npgsql;
using Shouldly;

namespace Kmic.Api.Tests.Configuration;

/// <summary>
/// 字典：启动同步、字典项的增改停用、缓存失效、ETag、Contracts，端到端
/// </summary>
public sealed class DictionaryAdministrationTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/configuration";

    private static readonly string[] _manage = [ConfigurationPermissions.DictionaryManage];

    private Task<HttpResponseMessage> CallAsync(HttpMethod method, string path, string token, object? body = null) =>
        SendAsync(method, Root + path, token, body);

    private async Task<long> VersionAsync(string token, string type) =>
        (await JsonAsync(await CallAsync(HttpMethod.Get, $"/dictionary-types/{type}", token))).GetProperty("version").GetInt64();

    private async Task<HttpResponseMessage> AddAsync(string token, string type, string code, string name, string? parent = null) =>
        await CallAsync(HttpMethod.Post, $"/dictionary-types/{type}/items", token, new { expectedVersion = await VersionAsync(token, type), code, name, parentCode = parent, sortOrder = (int?)null });

    private async Task<string[]> CodesAsync(string token, string type) =>
        [.. (await JsonAsync(await CallAsync(HttpMethod.Get, $"/dictionary-types/{type}", token))).GetProperty("items").EnumerateArray().Select(i => i.GetProperty("code").GetString()!)];

    private static async Task<string> CodeOfAsync(HttpResponseMessage response) => (await JsonAsync(response)).GetProperty("code").GetString()!;

    [Fact]
    public async Task TheDeclaredTypes_AreRegisteredAtStartup()
    {
        var admin = await LoginAsOperatorAsync("dict-list-admin", _manage);

        var response = await CallAsync(HttpMethod.Get, "/dictionary-types", admin);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var types = (await JsonAsync(response)).EnumerateArray().ToList();
        var category = types.Single(t => string.Equals(t.GetProperty("code").GetString(), SampleDictionaries.Category, StringComparison.Ordinal));
        category.GetProperty("displayName").GetString().ShouldBe("标本类别");
        category.GetProperty("ownerModule").GetString().ShouldBe("Sample");
        category.GetProperty("isHierarchical").GetBoolean().ShouldBeFalse();
        category.GetProperty("status").GetString().ShouldBe("Active");
        types.Single(t => string.Equals(t.GetProperty("code").GetString(), SampleDictionaries.Origin, StringComparison.Ordinal)).GetProperty("isHierarchical").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task AddingItems_ShowsThemInTheAdminViewAndTheReaderView_AndTheCacheIsInvalidated()
    {
        var admin = await LoginAsOperatorAsync("dict-add-admin", _manage);
        var reader = await LoginAsOperatorAsync("dict-add-reader", []);
        var type = SampleDictionaries.Category;
        (await CallAsync(HttpMethod.Get, $"/dictionaries?types={type}", reader)).StatusCode.ShouldBe(HttpStatusCode.OK);

        (await AddAsync(admin, type, "auto", "汽车")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await AddAsync(admin, type, "chem", "化工")).StatusCode.ShouldBe(HttpStatusCode.OK);

        var read = await JsonAsync(await CallAsync(HttpMethod.Get, $"/dictionaries?types={type}", reader));
        var items = read.GetProperty("dictionaries")[0].GetProperty("items").EnumerateArray().ToList();
        var codes = items.Select(i => i.GetProperty("code").GetString()).ToList();
        codes.Contains("auto", StringComparer.Ordinal).ShouldBeTrue();
        codes.IndexOf("auto").ShouldBeLessThan(codes.IndexOf("chem"));
        var sortOrders = items.Select(i => i.GetProperty("sortOrder").GetInt32()).ToList();
        sortOrders[codes.IndexOf("auto")].ShouldBeLessThan(sortOrders[codes.IndexOf("chem")]);
        items[codes.IndexOf("auto")].GetProperty("isActive").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task TheETag_GivesNotModified_UntilSomethingChanges()
    {
        var admin = await LoginAsOperatorAsync("dict-etag-admin", _manage);
        var type = SampleDictionaries.Origin;
        (await AddAsync(admin, type, "east", "华东")).StatusCode.ShouldBe(HttpStatusCode.OK);

        var first = await CallAsync(HttpMethod.Get, $"/dictionaries?types={type}", admin);
        var etag = first.Headers.ETag!.Tag;
        using var again = new HttpRequestMessage(HttpMethod.Get, $"{Root}/dictionaries?types={type}");
        again.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", admin);
        again.Headers.TryAddWithoutValidation("If-None-Match", etag);
        var notModified = await NewClient().SendAsync(again, Ct);
        (await AddAsync(admin, type, "south", "华南")).StatusCode.ShouldBe(HttpStatusCode.OK);
        using var afterChange = new HttpRequestMessage(HttpMethod.Get, $"{Root}/dictionaries?types={type}");
        afterChange.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", admin);
        afterChange.Headers.TryAddWithoutValidation("If-None-Match", etag);
        var changed = await NewClient().SendAsync(afterChange, Ct);

        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        notModified.StatusCode.ShouldBe(HttpStatusCode.NotModified);
        changed.StatusCode.ShouldBe(HttpStatusCode.OK);
        string.Equals(changed.Headers.ETag!.Tag, etag, StringComparison.Ordinal).ShouldBeFalse();
    }

    [Fact]
    public async Task ReadingUnknownTypes_LeavesThemOut_AndTooManyTypesIsRefused()
    {
        var reader = await LoginAsOperatorAsync("dict-unknown-reader", []);

        var unknown = await JsonAsync(await CallAsync(HttpMethod.Get, "/dictionaries?types=Nope.Nothing", reader));
        var tooMany = await CallAsync(HttpMethod.Get, "/dictionaries?types=" + string.Join(',', Enumerable.Range(0, 21).Select(i => "Sample.T" + i)), reader);

        unknown.GetProperty("dictionaries").GetArrayLength().ShouldBe(0);
        tooMany.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ItemRules_AreEnforced_CodesAreUniqueIgnoringCase_AndShapeIsChecked()
    {
        var admin = await LoginAsOperatorAsync("dict-rules-admin", _manage);
        var type = SampleDictionaries.Category;
        (await AddAsync(admin, type, "Rule.One", "第一项")).StatusCode.ShouldBe(HttpStatusCode.OK);

        var duplicate = await AddAsync(admin, type, "RULE.ONE", "同名不同大小写");
        var badShape = await AddAsync(admin, type, "has space", "编码里有空格");
        var withParent = await AddAsync(admin, type, "rule.two", "不分层的字典给了父项", parent: "Rule.One");

        duplicate.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(duplicate)).ShouldBe("Configuration.DictionaryItemCodeExists");
        badShape.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(badShape)).ShouldBe("Configuration.DictionaryItemCodeInvalid");
        withParent.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(withParent)).ShouldBe("Configuration.DictionaryParentInvalid");
    }

    [Fact]
    public async Task RenameDisableAndEnable_ChangeTheItem_AndAStaleVersionIsAConflict()
    {
        var admin = await LoginAsOperatorAsync("dict-edit-admin", _manage);
        var type = SampleDictionaries.Category;
        (await AddAsync(admin, type, "edit.me", "旧名")).StatusCode.ShouldBe(HttpStatusCode.OK);
        var version = await VersionAsync(admin, type);

        var renamed = await CallAsync(HttpMethod.Put, $"/dictionary-types/{type}/items/edit.me", admin, new { expectedVersion = version, name = "新名" });
        var stale = await CallAsync(HttpMethod.Put, $"/dictionary-types/{type}/items/edit.me", admin, new { expectedVersion = version, name = "又改" });
        var disabled = await CallAsync(HttpMethod.Post, $"/dictionary-types/{type}/items/edit.me/disable", admin, new { expectedVersion = await VersionAsync(admin, type) });
        var disabledAgain = await CallAsync(HttpMethod.Post, $"/dictionary-types/{type}/items/edit.me/disable", admin, new { expectedVersion = await VersionAsync(admin, type) });
        var enabled = await CallAsync(HttpMethod.Post, $"/dictionary-types/{type}/items/edit.me/enable", admin, new { expectedVersion = await VersionAsync(admin, type) });

        renamed.StatusCode.ShouldBe(HttpStatusCode.OK);
        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(stale)).ShouldBe("Common.VersionConflict");
        disabled.StatusCode.ShouldBe(HttpStatusCode.OK);
        disabledAgain.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        enabled.StatusCode.ShouldBe(HttpStatusCode.OK);
        var item = (await JsonAsync(await CallAsync(HttpMethod.Get, $"/dictionary-types/{type}", admin))).GetProperty("items").EnumerateArray().Single(i => string.Equals(i.GetProperty("code").GetString(), "edit.me", StringComparison.Ordinal));
        item.GetProperty("name").GetString().ShouldBe("新名");
        item.GetProperty("isActive").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task AHierarchicalDictionary_KeepsItsTreeRules()
    {
        var admin = await LoginAsOperatorAsync("dict-tree-admin", _manage);
        var type = SampleDictionaries.Origin;
        (await AddAsync(admin, type, "tree.cn", "中国")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await AddAsync(admin, type, "tree.sh", "上海", parent: "tree.cn")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await AddAsync(admin, type, "tree.pd", "浦东", parent: "tree.sh")).StatusCode.ShouldBe(HttpStatusCode.OK);

        var cycle = await CallAsync(HttpMethod.Post, $"/dictionary-types/{type}/items/tree.cn/move", admin, new { expectedVersion = await VersionAsync(admin, type), newParentCode = "tree.pd" });
        var withChildren = await CallAsync(HttpMethod.Post, $"/dictionary-types/{type}/items/tree.sh/disable", admin, new { expectedVersion = await VersionAsync(admin, type) });
        var moved = await CallAsync(HttpMethod.Post, $"/dictionary-types/{type}/items/tree.pd/move", admin, new { expectedVersion = await VersionAsync(admin, type), newParentCode = "tree.cn" });

        cycle.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(cycle)).ShouldBe("Configuration.DictionaryParentInvalid");
        withChildren.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(withChildren)).ShouldBe("Configuration.DictionaryItemHasEnabledChildren");
        moved.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pd = (await JsonAsync(await CallAsync(HttpMethod.Get, $"/dictionary-types/{type}", admin))).GetProperty("items").EnumerateArray().Single(i => string.Equals(i.GetProperty("code").GetString(), "tree.pd", StringComparison.Ordinal));
        pd.GetProperty("parentCode").GetString().ShouldBe("tree.cn");
    }

    [Fact]
    public async Task Reorder_NeedsTheWholeLevel()
    {
        var admin = await LoginAsOperatorAsync("dict-order-admin", _manage);
        var type = SampleDictionaries.Category;
        foreach (var code in new[] { "order.a", "order.b", "order.c" })
        {
            (await AddAsync(admin, type, code, code)).StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        var all = (await CodesAsync(admin, type)).ToList();
        var missing = await CallAsync(HttpMethod.Put, $"/dictionary-types/{type}/items-order", admin, new { expectedVersion = await VersionAsync(admin, type), parentCode = (string?)null, orderedCodes = all.Skip(1).ToArray() });
        var reversed = await CallAsync(HttpMethod.Put, $"/dictionary-types/{type}/items-order", admin, new { expectedVersion = await VersionAsync(admin, type), parentCode = (string?)null, orderedCodes = Enumerable.Reverse(all).ToArray() });

        missing.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(missing)).ShouldBe("Configuration.DictionaryOrderInvalid");
        reversed.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await CodesAsync(admin, type)).ShouldBe(Enumerable.Reverse(all).ToArray());
    }

    [Fact]
    public async Task ManagingNeedsThePermission_ButReadingDoesNot()
    {
        var nobody = await LoginAsOperatorAsync("dict-perm-nobody", []);
        var admin = await LoginAsOperatorAsync("dict-perm-admin", _manage);
        var type = SampleDictionaries.Category;
        var version = await VersionAsync(admin, type);

        var list = await CallAsync(HttpMethod.Get, "/dictionary-types", nobody);
        var detail = await CallAsync(HttpMethod.Get, $"/dictionary-types/{type}", nobody);
        var add = await CallAsync(HttpMethod.Post, $"/dictionary-types/{type}/items", nobody, new { expectedVersion = version, code = "nope", name = "不该成功", parentCode = (string?)null, sortOrder = (int?)null });
        var unknown = await CallAsync(HttpMethod.Get, "/dictionary-types/Nope.Nothing", admin);

        list.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        detail.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        add.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CodeOfAsync(unknown)).ShouldBe("Configuration.DictionaryTypeNotFound");
    }

    [Fact]
    public async Task OtherModules_ReadDictionariesThroughTheContract_IncludingWhatIsSelectable()
    {
        var admin = await LoginAsOperatorAsync("dict-contract-admin", _manage);
        var type = SampleDictionaries.Category;
        (await AddAsync(admin, type, "contract.on", "可选")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await AddAsync(admin, type, "contract.off", "停用")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await CallAsync(HttpMethod.Post, $"/dictionary-types/{type}/items/contract.off/disable", admin, new { expectedVersion = await VersionAsync(admin, type) })).StatusCode.ShouldBe(HttpStatusCode.OK);

        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope();
        var dictionaries = scope.ServiceProvider.GetRequiredService<IDictionaries>();
        var snapshot = await dictionaries.GetAsync(type, Ct);

        snapshot.ShouldNotBeNull().Items.Single(i => string.Equals(i.Code, "contract.off", StringComparison.Ordinal)).IsActive.ShouldBeFalse();
        (await dictionaries.IsSelectableAsync(type, "CONTRACT.ON", Ct)).ShouldBeTrue();
        (await dictionaries.IsSelectableAsync(type, "contract.off", Ct)).ShouldBeFalse();
        (await dictionaries.IsSelectableAsync(type, "ghost", Ct)).ShouldBeFalse();
        (await dictionaries.IsSelectableAsync("Nope.Nothing", "x", Ct)).ShouldBeFalse();
        (await dictionaries.GetAsync("Nope.Nothing", Ct)).ShouldBeNull();
    }

    [Fact]
    public async Task SyncingAgain_ChangesNothing_AndATypeRemovedFromTheCodeIsRetiredButKeepsItsData()
    {
        var admin = await LoginAsOperatorAsync("dict-sync-admin", _manage);
        await using (var data = NpgsqlDataSource.Create(ConnectionString))
        {
            await using var insert = data.CreateCommand("""
                INSERT INTO "Configuration"."DictionaryType" ("Id", "Code", "OwnerModule", "DisplayName", "IsHierarchical", "Status", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
                VALUES (@id, 'Sample.Removed', 'Sample', '已经删除的类型', false, 'Active', now(), '00000000-0000-0000-0000-000000000001', 0)
                """);
            insert.Parameters.AddWithValue("id", Guid.CreateVersion7());
            await insert.ExecuteNonQueryAsync(Ct);
        }

        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.Startup);
        var sender = scope.ServiceProvider.GetRequiredService<ICommandSender>();
        var first = (await sender.SendAsync(new SyncDictionaryTypesCommand(), Ct)).ShouldBeSuccess();
        var second = (await sender.SendAsync(new SyncDictionaryTypesCommand(), Ct)).ShouldBeSuccess();

        first.Retired.ShouldBe(1);
        first.Registered.ShouldBe(0);
        second.ShouldBe(new DictionarySyncResult(0, 0, 0));
        var removed = (await JsonAsync(await CallAsync(HttpMethod.Get, "/dictionary-types", admin))).EnumerateArray().Single(t => string.Equals(t.GetProperty("code").GetString(), "Sample.Removed", StringComparison.Ordinal));
        removed.GetProperty("status").GetString().ShouldBe("Retired");
        var add = await CallAsync(HttpMethod.Post, "/dictionary-types/Sample.Removed/items", admin, new { expectedVersion = removed.GetProperty("version").GetInt64(), code = "x", name = "不该成功", parentCode = (string?)null, sortOrder = (int?)null });
        add.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await CodeOfAsync(add)).ShouldBe("Configuration.DictionaryTypeRetired");
    }
}
