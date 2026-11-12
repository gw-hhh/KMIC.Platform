using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

    /// <summary>
    /// 执行服务容器里登记的全部模块的迁移，先做数据库级的准备，模块按名称顺序
    /// </summary>
    /// <remarks>
    /// 迁移程序和测试用的模板库不需要认识每个模块的 DbContext 类型：模块注册服务时已经登记了它
    /// </remarks>
    /// <param name="services">已经由各模块的 AddServices 注册过的服务容器</param>
    /// <param name="connectionString">迁移账号的连接串</param>
    /// <param name="ct">取消令牌</param>
    public static async Task MigrateAllAsync(IServiceProvider services, string connectionString, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);

        await PrepareDatabaseAsync(connectionString, ct);
        foreach (var registration in services.GetServices<ModuleWriteContext>().OrderBy(r => r.Module, StringComparer.Ordinal))
        {
            var migrate = typeof(ModuleMigrator).GetMethod(nameof(MigrateAsync))!.MakeGenericMethod(registration.ContextType);
            await (Task)migrate.Invoke(null, [connectionString, ct])!;
        }
    }

    /// <summary>
    /// 列出服务容器里登记的全部模块还没有执行的迁移，按模块名称排序
    /// </summary>
    /// <remarks>
    /// 启动时用运行账号检查：只读迁移历史表，不改库。模块的 schema 还不存在时，它的全部迁移都算未执行
    /// </remarks>
    /// <param name="services">已经由各模块的 AddServices 注册过的服务容器</param>
    /// <param name="connectionString">连接串</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>形如 Sample/20261112002136_InitializeModuleSchema 的迁移标识</returns>
    public static async Task<IReadOnlyList<string>> PendingAsync(IServiceProvider services, string connectionString, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);

        var pending = new List<string>();
        foreach (var registration in services.GetServices<ModuleWriteContext>().OrderBy(r => r.Module, StringComparer.Ordinal))
        {
            var query = typeof(ModuleMigrator).GetMethod(nameof(PendingOfAsync), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.MakeGenericMethod(registration.ContextType);
            var names = await (Task<IReadOnlyList<string>>)query.Invoke(null, [connectionString, ct])!;
            pending.AddRange(names.Select(name => $"{registration.Module}/{name}"));
        }

        return pending;
    }

    private static async Task<IReadOnlyList<string>> PendingOfAsync<TContext>(string connectionString, CancellationToken ct)
        where TContext : ModuleDbContext
    {
        await using var context = StandaloneContexts.Create<TContext>(connectionString);
        return [.. await context.Database.GetPendingMigrationsAsync(ct)];
    }
}
