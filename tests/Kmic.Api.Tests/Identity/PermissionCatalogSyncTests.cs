using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Identity.Application.Catalog;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 权限点与范围对象目录同步：启动同步、哈希跳过、改名、退役与恢复、全权限角色的指纹、定时清理
/// </summary>
public sealed class PermissionCatalogSyncTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Renamed = "Sample.Specimen.Close";
    private const string OldName = "Sample.Specimen.Finish";

    private async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CallOrigin origin = CallOrigin.Startup)
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(origin);
        return (await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(command, Ct)).ShouldBeSuccess();
    }

    // 宿主在第一次用到服务时才启动，启动时会同步一次目录：改数据库之前先把它启动起来，免得启动同步把准备好的现场抹掉
    private void StartHost() => Factory.Services.ShouldNotBeNull();

    // 让下一次同步认为目录变了
    private async Task MakeCatalogStaleAsync() =>
        await Seed.SqlAsync("""UPDATE "Identity"."CatalogState" SET "ContentHash" = 'stale'""");

    private string CurrentHash() =>
        CatalogContent.From(Factory.Services.GetRequiredService<PermissionCatalog>(), Factory.Services.GetRequiredService<DataScopeObjectCatalog>()).Hash();

    [Fact]
    public async Task TheStartupSync_RecordsEveryDeclaredPermissionAndScopeObject_AndTheHash()
    {
        var permissions = Factory.Services.GetRequiredService<PermissionCatalog>();
        var scopeObjects = Factory.Services.GetRequiredService<DataScopeObjectCatalog>();

        (await Seed.CountAsync("""SELECT count(*) FROM "Identity"."Permission" WHERE NOT "IsRetired" """)).ShouldBe(permissions.All.Count);
        (await Seed.CountAsync("""SELECT count(*) FROM "Identity"."DataScopeObject" WHERE NOT "IsRetired" """)).ShouldBe(scopeObjects.All.Count);
        (await Seed.ScalarAsync<string>("""SELECT "ContentHash" FROM "Identity"."CatalogState" WHERE "Name" = 'Permissions'""")).ShouldBe(CurrentHash());
        var sensitive = await Seed.ScalarAsync<bool>("""SELECT "IsSensitive" FROM "Identity"."Permission" WHERE "Code" = 'Identity.User.Impersonate'""");
        sensitive.ShouldBeTrue();
        (await WaitForSecurityEventsAsync("\"Type\" = 'PermissionCatalogSynced'")).ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task SyncingAgain_WithAnUnchangedCatalog_DoesNothing()
    {
        var result = await SendAsync(new SyncPermissionCatalogCommand());

        result.Changed.ShouldBeFalse();
        result.Changes.ShouldBeNull();
    }

    [Fact]
    public async Task ADriftedDatabase_IsBroughtBackInLineWithTheCode_AndWhatIsGoneIsRetiredNotDeleted()
    {
        StartHost();
        await Seed.SqlAsync("""UPDATE "Identity"."Permission" SET "DisplayName" = '旧的显示名' WHERE "Code" = 'Identity.Role.View'""");
        await Seed.SqlAsync("""DELETE FROM "Identity"."Permission" WHERE "Code" = 'Identity.Role.Manage'""");
        await Seed.SqlAsync(
            """
            INSERT INTO "Identity"."Permission" ("Id", "Code", "Module", "GroupName", "DisplayName", "IsSensitive", "IsRetired")
            VALUES (@id, 'Sample.Gone.Thing', 'Sample', '示例', '代码里没有了', false, false)
            """,
            ("id", Guid.CreateVersion7()));
        var role = await Seed.RoleAsync("留着退役配置的角色", ["Sample.Gone.Thing", "Identity.Role.View"]);
        await MakeCatalogStaleAsync();

        var result = await SendAsync(new SyncPermissionCatalogCommand());

        var changes = result.Changes.ShouldNotBeNull();
        result.Changed.ShouldBeTrue();
        changes.PermissionsAdded.ShouldBe(1);
        changes.PermissionsUpdated.ShouldBe(1);
        changes.PermissionsRetired.ShouldBe(1);
        (await Seed.ScalarAsync<string>("""SELECT "DisplayName" FROM "Identity"."Permission" WHERE "Code" = 'Identity.Role.View'""")).ShouldBe("查看角色");
        (await Seed.CountAsync("""SELECT count(*) FROM "Identity"."Permission" WHERE "Code" = 'Identity.Role.Manage' AND NOT "IsRetired" """)).ShouldBe(1);
        (await Seed.CountAsync("""SELECT count(*) FROM "Identity"."Permission" WHERE "Code" = 'Sample.Gone.Thing' AND "IsRetired" AND "RetiredAt" IS NOT NULL""")).ShouldBe(1);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Identity"."RolePermissionEntry" WHERE "RoleId" = '{role}' AND "PermissionCode" = 'Sample.Gone.Thing'""")).ShouldBe(1);
    }

    [Fact]
    public async Task ARetiredPermission_IsRestoredWhenTheCodeDeclaresItAgain()
    {
        StartHost();
        await Seed.SqlAsync("""UPDATE "Identity"."Permission" SET "IsRetired" = true, "RetiredAt" = now() WHERE "Code" = 'Identity.Role.View'""");
        await MakeCatalogStaleAsync();

        var result = await SendAsync(new SyncPermissionCatalogCommand());

        result.Changes.ShouldNotBeNull().PermissionsUpdated.ShouldBe(1);
        (await Seed.CountAsync("""SELECT count(*) FROM "Identity"."Permission" WHERE "Code" = 'Identity.Role.View' AND NOT "IsRetired" AND "RetiredAt" IS NULL""")).ShouldBe(1);
    }

    [Fact]
    public async Task ADeclaredRename_ChangesTheCodeInPlace_AndMovesTheRolesConfiguration()
    {
        StartHost();
        var id = Guid.CreateVersion7();
        await Seed.SqlAsync("""DELETE FROM "Identity"."Permission" WHERE "Code" = @code""", ("code", Renamed));
        await Seed.SqlAsync(
            """
            INSERT INTO "Identity"."Permission" ("Id", "Code", "Module", "GroupName", "DisplayName", "IsSensitive", "IsRetired")
            VALUES (@id, @code, 'Sample', '示例', '关闭示例标本', false, false)
            """,
            ("id", id), ("code", OldName));
        var role = await Seed.RoleAsync("配置了旧编码的角色", [OldName, "Sample.Specimen.View"]);
        var other = await Seed.RoleAsync("两个编码都配置了的角色", [OldName, Renamed]);
        var versionBefore = await Seed.ScalarAsync<long>($"""SELECT "ConcurrencyVersion" FROM "Identity"."Role" WHERE "Id" = '{role}'""");
        await MakeCatalogStaleAsync();

        var result = await SendAsync(new SyncPermissionCatalogCommand());

        result.Changes.ShouldNotBeNull().Renames.ShouldContainKeyAndValue(OldName, Renamed);
        (await Seed.ScalarAsync<Guid>("""SELECT "Id" FROM "Identity"."Permission" WHERE "Code" = @code""", ("code", Renamed))).ShouldBe(id);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Identity"."Permission" WHERE "Code" = '{OldName}'""")).ShouldBe(0);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Identity"."RolePermissionEntry" WHERE "RoleId" = '{role}' AND "PermissionCode" = '{Renamed}'""")).ShouldBe(1);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Identity"."RolePermissionEntry" WHERE "PermissionCode" = '{OldName}'""")).ShouldBe(0);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Identity"."RolePermissionEntry" WHERE "RoleId" = '{other}'""")).ShouldBe(1);
        (await Seed.ScalarAsync<long>($"""SELECT "ConcurrencyVersion" FROM "Identity"."Role" WHERE "Id" = '{role}'""")).ShouldBe(versionBefore + 1);
    }

    [Fact]
    public async Task ARoleThatGrantsEverything_GetsTheNewFingerprint_AndItsAuthorizationVersionMoves()
    {
        StartHost();
        var all = await Seed.RoleAsync("全权限角色", [], builtIn: true);
        var versionBefore = await Seed.ScalarAsync<long>($"""SELECT "ConcurrencyVersion" FROM "Identity"."Role" WHERE "Id" = '{all}'""");
        await MakeCatalogStaleAsync();

        await SendAsync(new SyncPermissionCatalogCommand());

        (await Seed.ScalarAsync<string>($"""SELECT "CatalogFingerprint" FROM "Identity"."Role" WHERE "Id" = '{all}'""")).ShouldBe(CurrentHash());
        (await Seed.ScalarAsync<long>($"""SELECT "ConcurrencyVersion" FROM "Identity"."Role" WHERE "Id" = '{all}'""")).ShouldBe(versionBefore + 1);
    }

    [Fact]
    public async Task ThePurge_RemovesWhatHasBeenRetiredForLong_TogetherWithTheRolesConfigurationOfIt()
    {
        StartHost();
        await InsertRetiredAsync("Sample.Old.Thing", daysAgo: 200);
        await InsertRetiredAsync("Sample.Recent.Thing", daysAgo: 10);
        await Seed.SqlAsync(
            """
            INSERT INTO "Identity"."DataScopeObject" ("Id", "Code", "Module", "DisplayName", "IsRetired", "RetiredAt")
            VALUES (@id, 'Sample.Old', 'Sample', '很久以前的范围对象', true, now() - interval '200 days')
            """,
            ("id", Guid.CreateVersion7()));
        var role = await Seed.RoleAsync("配置了退役项的角色", ["Sample.Old.Thing", "Sample.Recent.Thing", "Sample.Specimen.View"]);
        await Seed.SqlAsync(
            """
            INSERT INTO "Identity"."RoleDataScope" ("Id", "ScopeObject", "Level", "RoleId", "CreatedAt", "CreatedBy")
            VALUES (@id, 'Sample.Old', 'Self', @role, now(), '00000000-0000-0000-0000-000000000001')
            """,
            ("id", Guid.CreateVersion7()), ("role", role));

        var outcome = await SendAsync(new PurgeRetiredPermissionsCommand(TimeProvider.System.GetUtcNow()), CallOrigin.ScheduledJob);

        outcome.ItemsProcessed.ShouldBe(2);
        (await Seed.CountAsync("""SELECT count(*) FROM "Identity"."Permission" WHERE "Code" = 'Sample.Old.Thing'""")).ShouldBe(0);
        (await Seed.CountAsync("""SELECT count(*) FROM "Identity"."Permission" WHERE "Code" = 'Sample.Recent.Thing'""")).ShouldBe(1);
        (await Seed.CountAsync("""SELECT count(*) FROM "Identity"."DataScopeObject" WHERE "Code" = 'Sample.Old'""")).ShouldBe(0);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Identity"."RolePermissionEntry" WHERE "RoleId" = '{role}' AND "PermissionCode" = 'Sample.Old.Thing'""")).ShouldBe(0);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Identity"."RolePermissionEntry" WHERE "RoleId" = '{role}'""")).ShouldBe(2);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Identity"."RoleDataScope" WHERE "RoleId" = '{role}'""")).ShouldBe(0);
    }

    private async Task InsertRetiredAsync(string code, int daysAgo) =>
        await Seed.SqlAsync(
            """
            INSERT INTO "Identity"."Permission" ("Id", "Code", "Module", "GroupName", "DisplayName", "IsSensitive", "IsRetired", "RetiredAt")
            VALUES (@id, @code, 'Sample', '示例', '退役的权限点', false, true, now() - make_interval(days => @days))
            """,
            ("id", Guid.CreateVersion7()), ("code", code), ("days", daysAgo));
}
