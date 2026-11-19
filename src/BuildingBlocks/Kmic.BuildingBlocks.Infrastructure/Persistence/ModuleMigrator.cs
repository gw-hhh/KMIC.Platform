using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 模块迁移，用迁移账号执行
/// </summary>
/// <remarks>
/// 只给迁移程序和测试模板库用
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

    /// <summary>
    /// 列出实体模型和最新迁移不一致的模块，即改了模型却没有生成迁移
    /// </summary>
    /// <returns>有差异的模块名，按名称排序</returns>
    public static IReadOnlyList<string> ModulesWithPendingModelChanges(IServiceProvider services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);

        var changed = new List<string>();
        foreach (var registration in services.GetServices<ModuleWriteContext>().OrderBy(r => r.Module, StringComparer.Ordinal))
        {
            var check = typeof(ModuleMigrator).GetMethod(nameof(HasPendingModelChangesOf), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.MakeGenericMethod(registration.ContextType);
            if ((bool)check.Invoke(null, [connectionString])!)
            {
                changed.Add(registration.Module);
            }
        }

        return changed;
    }

    private static bool HasPendingModelChangesOf<TContext>(string connectionString)
        where TContext : ModuleDbContext
    {
        using var context = StandaloneContexts.Create<TContext>(connectionString);
        return context.Database.HasPendingModelChanges();
    }

    private static async Task<IReadOnlyList<string>> PendingOfAsync<TContext>(string connectionString, CancellationToken ct)
        where TContext : ModuleDbContext
    {
        await using var context = StandaloneContexts.Create<TContext>(connectionString);
        return [.. await context.Database.GetPendingMigrationsAsync(ct)];
    }
}
