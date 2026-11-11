using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 模块迁移
/// </summary>
/// <remarks>
/// 用迁移账号执行，只给迁移程序和测试模板库用。应用启动时只检查有没有未执行的迁移
/// </remarks>
public static class ModuleMigrator
{
    /// <summary>
    /// 执行数据库级的准备，要在所有模块迁移之前
    /// </summary>
    public static async Task PrepareDatabaseAsync(string connectionString, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await CaseInsensitiveCollation.EnsureAsync(connection, ct);
    }

    /// <summary>
    /// 执行模块迁移
    /// </summary>
    public static async Task MigrateAsync<TContext>(string connectionString, CancellationToken ct)
        where TContext : ModuleDbContext
    {
        await using var context = StandaloneContexts.Create<TContext>(connectionString);
        await context.Database.MigrateAsync(ct);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await CaseInsensitiveCollation.VerifyColumnsAsync(connection, context, ct);
    }
}
