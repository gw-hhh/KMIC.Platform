using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 启动时核对数据库结构
/// </summary>
/// <remarks>
/// 开发环境自动执行全部迁移；其他环境只检查，发现有未执行的迁移就拒绝启动，避免新代码跑在旧表结构上。
/// 排在所有启动任务之前：后面的任务都要读写表
/// </remarks>
internal sealed class MigrationCheckTask(
    IServiceProvider services,
    IOptions<DatabaseOptions> options,
    ILogger<MigrationCheckTask> logger) : IStartupTask
{
    public string Name => "Framework.MigrationCheck";

    public int Order => -1000;

    public async ValueTask<Result> ExecuteAsync(CancellationToken ct)
    {
        var database = options.Value;
        var modules = services.GetServices<ModuleWriteContext>().Count();
        if (database.AutoMigrate)
        {
            var migrationAccount = string.IsNullOrWhiteSpace(database.MigrationConnectionString)
                ? database.ConnectionString
                : database.MigrationConnectionString;
            await ModuleMigrator.MigrateAllAsync(services, migrationAccount, ct);
            HostingLog.MigrationsApplied(logger, modules);
            return Result.Success();
        }

        var pending = await ModuleMigrator.PendingAsync(services, database.ConnectionString, ct);
        if (pending.Count > 0)
        {
            throw new InvalidOperationException(
                $"数据库有 {pending.Count} 个未执行的迁移，拒绝启动：{string.Join("、", pending)}。请先运行 Kmic.MigrationRunner。");
        }

        HostingLog.MigrationsUpToDate(logger, modules);
        return Result.Success();
    }
}
