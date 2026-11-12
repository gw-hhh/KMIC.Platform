using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// 直接往 Identity 的表里造测试数据：管理用例还没有接进来之前，认证流程的测试用它准备用户、角色和授予
/// </summary>
/// <remarks>
/// 密码哈希用和产品代码同一套哈希器（PBKDF2，ASP.NET Core 的 PasswordHasher），所以登录时能被识别
/// </remarks>
internal sealed class IdentitySeed(string connectionString)
{
    private static readonly Guid _system = new("00000000-0000-0000-0000-000000000001");

    private readonly PasswordHasher<object> _hasher = new();

    public async Task<Guid> RoleAsync(string name, IReadOnlyCollection<string> permissions, string scope = "All", int sortOrder = 10, bool active = true, bool isProtected = false, bool builtIn = false)
    {
        var id = Guid.CreateVersion7();
        await using var data = NpgsqlDataSource.Create(connectionString);
        await ExecuteAsync(
            data,
            """
            INSERT INTO "Identity"."Role" ("Id", "Name", "Status", "IsProtected", "GrantsAllPermissions", "DefaultScopeLevel", "SortOrder", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            VALUES (@id, @name, @status, @protected, @builtin, @scope, @sort, now(), @system, 0)
            """,
            ("id", id), ("name", name), ("status", active ? "Active" : "Disabled"), ("scope", scope), ("sort", sortOrder), ("protected", isProtected), ("builtin", builtIn), ("system", _system));
        foreach (var permission in permissions)
        {
            await ExecuteAsync(
                data,
                """
                INSERT INTO "Identity"."RolePermissionEntry" ("Id", "PermissionCode", "RoleId", "CreatedAt", "CreatedBy")
                VALUES (@id, @code, @role, now(), @system)
                """,
                ("id", Guid.CreateVersion7()), ("code", permission), ("role", id), ("system", _system));
        }

        return id;
    }

    public async Task<Guid> UserAsync(string account, string? password, string displayName = "测试用户", string? mobile = null, bool active = true, bool mustChange = false)
    {
        var id = Guid.CreateVersion7();
        await using var data = NpgsqlDataSource.Create(connectionString);
        await ExecuteAsync(
            data,
            """
            INSERT INTO "Identity"."User" ("Id", "Account", "DisplayName", "Mobile", "Kind", "Status", "PasswordHash", "PasswordChangeRequired", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            VALUES (@id, @account, @name, @mobile, 'Normal', @status, @hash, @must, now(), @system, 0)
            """,
            ("id", id), ("account", account), ("name", displayName), ("mobile", (object?)mobile ?? DBNull.Value), ("status", active ? "Active" : "Disabled"),
            ("hash", password is null ? DBNull.Value : _hasher.HashPassword(new object(), password)), ("must", mustChange), ("system", _system));
        return id;
    }

    public async Task GrantAsync(Guid userId, Guid roleId)
    {
        await using var data = NpgsqlDataSource.Create(connectionString);
        await ExecuteAsync(
            data,
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
        await using var data = NpgsqlDataSource.Create(connectionString);
        await ExecuteAsync(
            data,
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
        await using var command = data.CreateCommand("""SELECT "Id" FROM "Identity"."OrgUnit" WHERE "ParentId" IS NULL""");
        return (Guid)(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken))!;
    }

    public async Task SetRoleStatusAsync(Guid roleId, bool active)
    {
        await using var data = NpgsqlDataSource.Create(connectionString);
        await ExecuteAsync(
            data,
            """UPDATE "Identity"."Role" SET "Status" = @status, "ConcurrencyVersion" = "ConcurrencyVersion" + 1 WHERE "Id" = @id""",
            ("status", active ? "Active" : "Disabled"), ("id", roleId));
    }

    public async Task<string?> SessionStatusAsync(Guid sessionId)
    {
        await using var data = NpgsqlDataSource.Create(connectionString);
        await using var command = data.CreateCommand("""SELECT "Status" FROM "Identity"."Session" WHERE "Id" = @id""");
        command.Parameters.AddWithValue("id", sessionId);
        return await command.ExecuteScalarAsync(TestContext.Current.CancellationToken) as string;
    }

    public async Task<long> CountAsync(string sql)
    {
        await using var data = NpgsqlDataSource.Create(connectionString);
        await using var command = data.CreateCommand(sql);
        return (long)(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken) ?? 0L);
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
