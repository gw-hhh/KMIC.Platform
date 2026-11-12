using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.Tools.Tests;

/// <summary>
/// 首个管理员初始化与应急重置：真实数据库，先用迁移程序建好结构，再跑运维命令
/// </summary>
public sealed class IdentityOperatorTests(ToolsTemplate template) : IAsyncLifetime
{
    private const string Password = "correct horse battery";

    private readonly ClassDatabase<ToolsTemplate> _database = new(template);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();
        (await ToolHarness.RunnerAsync(_database.ConnectionString, production: false, "migrate")).ExitCode.ShouldBe(0);
    }

    public ValueTask DisposeAsync() => _database.DisposeAsync();

    private async Task<long> CountAsync(string sql)
    {
        await using var command = _database.DataSource.CreateCommand(sql);
        return (long)(await command.ExecuteScalarAsync(Ct) ?? 0L);
    }

    private async Task<(int ExitCode, string Output)> BootstrapAsync(string password = Password, string account = "admin") =>
        await ToolHarness.OperatorAsync(_database.ConnectionString, password + Environment.NewLine, "identity", "bootstrap", "--account", account, "--name", "系统管理员甲");

    [Fact]
    public async Task Bootstrap_CreatesTheAdministrator_TheProtectedRole_TheGrant_AndTheTreeRoot()
    {
        var (exitCode, output) = await BootstrapAsync();

        exitCode.ShouldBe(0, output);
        (await CountAsync("""SELECT count(*) FROM "Identity"."User" WHERE "Account" = 'admin' AND "Kind" = 'Normal' AND "PasswordChangeRequired" AND "PasswordHash" IS NOT NULL""")).ShouldBe(1);
        (await CountAsync("""SELECT count(*) FROM "Identity"."Role" WHERE "IsProtected" AND "GrantsAllPermissions" AND "Status" = 'Active'""")).ShouldBe(1);
        (await CountAsync("""SELECT count(*) FROM "Identity"."UserRoleGrant" g JOIN "Identity"."User" u ON u."Id" = g."UserId" WHERE u."Account" = 'admin'""")).ShouldBe(1);
        (await CountAsync("""SELECT count(*) FROM "Identity"."OrgUnit" WHERE "ParentId" IS NULL AND "Name" = '总部'""")).ShouldBe(1);
        (await CountAsync("""SELECT count(*) FROM "Identity"."SecurityEvent" WHERE "Type" = 'BootstrapExecuted'""")).ShouldBe(1);
        (await CountAsync("""SELECT count(*) FROM "Identity"."UserStateHistory" """)).ShouldBeGreaterThan(0);
        output.ShouldNotContain(Password);
    }

    [Fact]
    public async Task Bootstrap_WhenAUserAlreadyExists_IsRefused_AndChangesNothing()
    {
        (await BootstrapAsync()).ExitCode.ShouldBe(0);

        var (exitCode, output) = await BootstrapAsync(account: "another");

        exitCode.ShouldBe(1);
        output.ShouldContain("Identity.AlreadyBootstrapped");
        (await CountAsync("""SELECT count(*) FROM "Identity"."User" WHERE "Kind" = 'Normal'""")).ShouldBe(1);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("password123")]
    public async Task Bootstrap_WithAWeakPassword_IsRefused(string password)
    {
        var (exitCode, output) = await BootstrapAsync(password);

        exitCode.ShouldBe(1);
        output.ShouldContain("Identity.PasswordTooWeak");
        (await CountAsync("""SELECT count(*) FROM "Identity"."User" WHERE "Kind" = 'Normal'""")).ShouldBe(0);
    }

    [Fact]
    public async Task Bootstrap_WithoutAPasswordOnStandardInput_FailsWithAClearMessage()
    {
        var (exitCode, output) = await ToolHarness.OperatorAsync(_database.ConnectionString, string.Empty, "identity", "bootstrap", "--account", "admin", "--name", "甲");

        exitCode.ShouldBe(1);
        output.ShouldContain("标准输入");
    }

    [Fact]
    public async Task Bootstrap_WithoutAConnectionString_FailsWithAClearMessage()
    {
        var (exitCode, output) = await ToolHarness.OperatorAsync(null, Password + Environment.NewLine, "identity", "bootstrap", "--account", "admin", "--name", "甲");

        exitCode.ShouldBe(1);
        output.ShouldContain("Kmic:Operator:ConnectionString");
    }

    [Fact]
    public async Task ResetAdminPassword_ReplacesThePassword_RevokesSessions_ClearsTheThrottle_AndLeavesATrace()
    {
        (await BootstrapAsync()).ExitCode.ShouldBe(0);
        await using (var seed = _database.DataSource.CreateCommand(
            """
            INSERT INTO "Identity"."Session" ("Id", "UserId", "CurrentRoleId", "ClientType", "Status", "LastRefreshedAt", "RefreshExpiresAt", "AbsoluteExpiresAt", "ImpersonationAllowsWrite", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            SELECT gen_random_uuid(), u."Id", g."RoleId", 'Web', 'Active', now(), now() + interval '1 day', now() + interval '1 day', false, now(), u."Id", 0
            FROM "Identity"."User" u JOIN "Identity"."UserRoleGrant" g ON g."UserId" = u."Id" WHERE u."Account" = 'admin';
            INSERT INTO "Identity"."LoginThrottle" ("LoginNameHash", "FailureCount", "LockLevel", "LastFailedAt") VALUES ('x', 9, 0, now());
            """))
        {
            await seed.ExecuteNonQueryAsync(Ct);
        }

        var hashBefore = await HashOfAsync();
        var (exitCode, output) = await ToolHarness.OperatorAsync(
            _database.ConnectionString, "another correct passphrase" + Environment.NewLine, "identity", "reset-admin-password", "--account", "admin");

        exitCode.ShouldBe(0, output);
        string.Equals(await HashOfAsync(), hashBefore, StringComparison.Ordinal).ShouldBeFalse();
        (await CountAsync("""SELECT count(*) FROM "Identity"."Session" WHERE "Status" = 'Active'""")).ShouldBe(0);
        (await CountAsync("""SELECT count(*) FROM "Identity"."Session" WHERE "Status" = 'Revoked' AND "RevokedReason" = 'PasswordReset'""")).ShouldBe(1);
        (await CountAsync("""SELECT count(*) FROM "Identity"."SecurityEvent" WHERE "Type" = 'PasswordReset'""")).ShouldBe(1);
        (await CountAsync("""SELECT count(*) FROM "Identity"."User" WHERE "Account" = 'admin' AND "PasswordChangeRequired" """)).ShouldBe(1);
    }

    [Fact]
    public async Task ResetAdminPassword_ForAnOrdinaryUser_IsRefused()
    {
        (await BootstrapAsync()).ExitCode.ShouldBe(0);
        await using (var seed = _database.DataSource.CreateCommand(
            """
            INSERT INTO "Identity"."User" ("Id", "Account", "DisplayName", "Kind", "Status", "PasswordChangeRequired", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            VALUES (gen_random_uuid(), 'ordinary', '普通用户', 'Normal', 'Active', false, now(), '00000000-0000-0000-0000-000000000001', 0)
            """))
        {
            await seed.ExecuteNonQueryAsync(Ct);
        }

        var (exitCode, output) = await ToolHarness.OperatorAsync(
            _database.ConnectionString, "another correct passphrase" + Environment.NewLine, "identity", "reset-admin-password", "--account", "ordinary");

        exitCode.ShouldBe(1);
        output.ShouldContain("Identity.NotProtectedHolder");
    }

    [Fact]
    public async Task ResetAdminPassword_ForAnUnknownAccount_IsRefused()
    {
        var (exitCode, output) = await ToolHarness.OperatorAsync(
            _database.ConnectionString, "another correct passphrase" + Environment.NewLine, "identity", "reset-admin-password", "--account", "nobody");

        exitCode.ShouldBe(1);
        output.ShouldContain("Identity.UserNotFound");
    }

    private async Task<string> HashOfAsync()
    {
        await using var command = _database.DataSource.CreateCommand("""SELECT "PasswordHash" FROM "Identity"."User" WHERE "Account" = 'admin'""");
        return (string)(await command.ExecuteScalarAsync(Ct))!;
    }
}
