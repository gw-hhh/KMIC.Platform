using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>模块数据库迁移执行器（使用迁移账号，仅供迁移程序与测试库使用）</summary>
public static class ModuleMigrator
{
    /// <summary>执行数据库级前置初始化，须在所有模块迁移之前调用</summary>
    public static async Task PrepareDatabaseAsync(string connectionString, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await CaseInsensitiveCollation.EnsureAsync(connection, ct);
    }

    /// <summary>执行单个模块的迁移</summary>
    public static async Task MigrateAsync<TContext>(string connectionString, CancellationToken ct)
        where TContext : ModuleDbContext
    {
        await using var context = StandaloneContexts.Create<TContext>(connectionString);
        await context.Database.MigrateAsync(ct);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await CaseInsensitiveCollation.VerifyColumnsAsync(connection, context, ct);
    }

    /// <summary>先完成数据库级初始化，再按模块名顺序执行全部已注册模块的迁移</summary>
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

    /// <summary>按模块名顺序列出待执行的迁移（格式如 Sample/20261112002136_InitializeModuleSchema）</summary>
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

    /// <summary>列出模型与最新迁移快照存在差异的模块（按名称排序）</summary>
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
