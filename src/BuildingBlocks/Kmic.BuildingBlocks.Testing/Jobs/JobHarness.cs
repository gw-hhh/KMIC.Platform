using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Testing.Jobs;

/// <summary>测试进程内后台作业执行辅助工具。</summary>
public static class JobHarness
{
    /// <summary>依次执行所有模块中的排队作业直至队列为空，返回累计执行数量。</summary>
    public static async Task<int> RunJobsAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        var executor = services.GetRequiredService<JobExecutor>();
        var total = 0;
        foreach (var module in services.GetRequiredService<ModuleFrameworkTables>().Modules)
        {
            int count;
            while ((count = await executor.RunBatchAsync(module, 1, ct)) > 0)
            {
                total += count;
            }
        }

        return total;
    }
}
