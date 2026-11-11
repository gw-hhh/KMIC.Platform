using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 以迁移账号执行迁移：先做数据库级的框架步骤，再逐个模块迁移，最后核对不区分大小写的列
/// </summary>
/// <remarks>
/// 只由迁移程序（与测试的模板库）使用；应用启动时只检查有没有未执行的迁移
/// </remarks>
public static class ModuleMigrator
{
    /// <summary>
    /// 数据库级的框架步骤：创建并核对 public."CaseInsensitive"；在任何模块迁移之前执行
    /// </summary>
    public static async Task PrepareDatabaseAsync(string connectionString, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await CaseInsensitiveCollation.EnsureAsync(connection, ct);
    }

    /// <summary>
    /// 执行一个模块的全部迁移，并核对该模块不区分大小写的列
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
