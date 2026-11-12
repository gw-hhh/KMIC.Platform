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

    public async Task<Guid> RoleAsync(string name, IReadOnlyCollection<string> permissions, string scope = "All", int sortOrder = 10, bool active = true)
    {
        var id = Guid.CreateVersion7();
        await using var data = NpgsqlDataSource.Create(connectionString);
        await ExecuteAsync(
            data,
            """
            INSERT INTO "Identity"."Role" ("Id", "Name", "Status", "IsProtected", "GrantsAllPermissions", "DefaultScopeLevel", "SortOrder", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            VALUES (@id, @name, @status, false, false, @scope, @sort, now(), @system, 0)
            """,
            ("id", id), ("name", name), ("status", active ? "Active" : "Disabled"), ("scope", scope), ("sort", sortOrder), ("system", _system));
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
