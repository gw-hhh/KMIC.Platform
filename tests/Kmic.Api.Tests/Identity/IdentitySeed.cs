using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.Api.Tests.Identity;

// 直接向 Identity 表预置测试数据，使用与业务一致的 PasswordHasher（PBKDF2）
internal sealed class IdentitySeed(string connectionString) : IAsyncDisposable
{
    private static readonly Guid _system = new("00000000-0000-0000-0000-000000000001");

    // 每个测试实例复用单个连接池：若每次写入均重建连接池并关闭，大量测试连续运行会耗尽本机临时端口
    private readonly NpgsqlDataSource _data = NpgsqlDataSource.Create(connectionString);
    // 故意固定较低的旧版迭代次数，以便在登录时触发业务代码的密码哈希自动升级逻辑
    internal const int SeedIterationCount = 100_000;

    private readonly PasswordHasher<object> _hasher = new(Options.Create(new PasswordHasherOptions { IterationCount = SeedIterationCount }));

    public async ValueTask DisposeAsync() => await _data.DisposeAsync();

    public async Task<Guid> RoleAsync(string name, IReadOnlyCollection<string> permissions, string scope = "All", int sortOrder = 10, bool active = true, bool isProtected = false, bool builtIn = false)
    {
        var id = Guid.CreateVersion7();
        await ExecuteAsync(
            _data,
            """
            INSERT INTO "Identity"."Role" ("Id", "Name", "Status", "IsProtected", "GrantsAllPermissions", "DefaultScopeLevel", "SortOrder", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            VALUES (@id, @name, @status, @protected, @builtin, @scope, @sort, now(), @system, 0)
            """,
            ("id", id), ("name", name), ("status", active ? "Active" : "Disabled"), ("scope", scope), ("sort", sortOrder), ("protected", isProtected), ("builtin", builtIn), ("system", _system));
        foreach (var permission in permissions)
        {
            await ExecuteAsync(
                _data,
                """
                INSERT INTO "Identity"."RolePermissionEntry" ("Id", "PermissionCode", "RoleId", "CreatedAt", "CreatedBy")
                VALUES (@id, @code, @role, now(), @system)
                """,
                ("id", Guid.CreateVersion7()), ("code", permission), ("role", id), ("system", _system));
        }

        return id;
    }

    public async Task<Guid> UserAsync(string account, string? password, string displayName = "测试用户", string? mobile = null, bool active = true, bool mustChange = false, string? employeeNo = null, string? email = null, Guid? id = null)
    {
        id ??= Guid.CreateVersion7();
        await ExecuteAsync(
            _data,
            """
            INSERT INTO "Identity"."User" ("Id", "Account", "DisplayName", "Mobile", "EmployeeNo", "Email", "Kind", "Status", "PasswordHash", "PasswordChangeRequired", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            VALUES (@id, @account, @name, @mobile, @employee, @email, 'Normal', @status, @hash, @must, now(), @system, 0)
            """,
            ("id", id.Value), ("account", account), ("name", displayName), ("mobile", (object?)mobile ?? DBNull.Value), ("employee", (object?)employeeNo ?? DBNull.Value), ("email", (object?)email ?? DBNull.Value), ("status", active ? "Active" : "Disabled"),
            ("hash", password is null ? DBNull.Value : _hasher.HashPassword(new object(), password)), ("must", mustChange), ("system", _system));
        return id.Value;
    }

    public async Task GrantAsync(Guid userId, Guid roleId)
    {
        await ExecuteAsync(
            _data,
            """
            INSERT INTO "Identity"."UserRoleGrant" ("Id", "RoleId", "GrantedBy", "GrantedAt", "CreatedAt", "CreatedBy", "UserId")
            VALUES (@id, @role, @system, now(), now(), @system, @user)
            """,
            ("id", Guid.CreateVersion7()), ("role", roleId), ("user", userId), ("system", _system));
    }

    public async Task<Guid> OrgTreeAsync(string rootName = "总部")
    {
        await ExecuteAsync(
            _data,
            """
            WITH tree AS (
                INSERT INTO "Identity"."OrgTree" ("Id", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
                VALUES ('00000000-0000-0000-0000-000000000003', now(), @system, 0)
                ON CONFLICT DO NOTHING
                RETURNING "Id")
            INSERT INTO "Identity"."OrgUnit" ("Id", "ParentId", "Name", "Code", "Status", "SortOrder", "CreatedAt", "CreatedBy", "TreeId")
            SELECT @unit, NULL, @name, NULL, 'Active', 1000, now(), @system, "Id" FROM tree
            """,
            ("unit", Guid.CreateVersion7()), ("name", rootName), ("system", _system));
        await using var command = _data.CreateCommand("""SELECT "Id" FROM "Identity"."OrgUnit" WHERE "ParentId" IS NULL""");
        return (Guid)(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken))!;
    }

    public async Task SetRoleStatusAsync(Guid roleId, bool active)
    {
        await ExecuteAsync(
            _data,
            """UPDATE "Identity"."Role" SET "Status" = @status, "ConcurrencyVersion" = "ConcurrencyVersion" + 1 WHERE "Id" = @id""",
            ("status", active ? "Active" : "Disabled"), ("id", roleId));
    }

    public async Task<string?> SessionStatusAsync(Guid sessionId)
    {
        await using var command = _data.CreateCommand("""SELECT "Status" FROM "Identity"."Session" WHERE "Id" = @id""");
        command.Parameters.AddWithValue("id", sessionId);
        return await command.ExecuteScalarAsync(TestContext.Current.CancellationToken) as string;
    }

    public async Task<long> CountAsync(string sql)
    {
        await using var command = _data.CreateCommand(sql);
        return (long)(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken) ?? 0L);
    }

    public async Task SqlAsync(string sql, params (string Name, object Value)[] parameters) => await ExecuteAsync(_data, sql, parameters);

    public async Task<T?> ScalarAsync<T>(string sql, params (string Name, object Value)[] parameters)
    {
        await using var command = _data.CreateCommand(sql);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        var result = await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);
        return result is null or DBNull ? default : (T)result;
    }

    private static async Task ExecuteAsync(NpgsqlDataSource data, string sql, params (string Name, object Value)[] parameters)
    {
        await using var command = data.CreateCommand(sql);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
    }
}
