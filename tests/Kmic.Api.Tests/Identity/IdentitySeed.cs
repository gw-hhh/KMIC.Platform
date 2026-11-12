using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 直接往 Identity 的表里造测试数据：管理用例还没有接进来之前，认证流程的测试用它准备用户、角色和授予
/// </summary>
/// <remarks>
/// 密码哈希用和产品代码同一套哈希器（PBKDF2，ASP.NET Core 的 PasswordHasher），所以登录时能被识别
/// </remarks>
internal sealed class IdentitySeed(string connectionString) : IAsyncDisposable
{
    private static readonly Guid _system = new("00000000-0000-0000-0000-000000000001");

    // 一个测试只开一个连接池：每次造数据都新建连接池，连接用完即关，测试多了会把本机的临时端口用光
    private readonly NpgsqlDataSource _data = NpgsqlDataSource.Create(connectionString);
    private readonly PasswordHasher<object> _hasher = new();

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

    public async Task<Guid> UserAsync(string account, string? password, string displayName = "测试用户", string? mobile = null, bool active = true, bool mustChange = false, string? employeeNo = null, string? email = null)
    {
        var id = Guid.CreateVersion7();
        await ExecuteAsync(
            _data,
            """
            INSERT INTO "Identity"."User" ("Id", "Account", "DisplayName", "Mobile", "EmployeeNo", "Email", "Kind", "Status", "PasswordHash", "PasswordChangeRequired", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            VALUES (@id, @account, @name, @mobile, @employee, @email, 'Normal', @status, @hash, @must, now(), @system, 0)
            """,
            ("id", id), ("account", account), ("name", displayName), ("mobile", (object?)mobile ?? DBNull.Value), ("employee", (object?)employeeNo ?? DBNull.Value), ("email", (object?)email ?? DBNull.Value), ("status", active ? "Active" : "Disabled"),
            ("hash", password is null ? DBNull.Value : _hasher.HashPassword(new object(), password)), ("must", mustChange), ("system", _system));
        return id;
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

    /// <summary>
    /// 建好组织树和根单元（已经有就不动），返回根单元的 ID
    /// </summary>
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

    /// <summary>
    /// 执行一条任意 SQL（造数据或改数据）
    /// </summary>
    public async Task SqlAsync(string sql, params (string Name, object Value)[] parameters) => await ExecuteAsync(_data, sql, parameters);

    /// <summary>
    /// 读一个值
    /// </summary>
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
