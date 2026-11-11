using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Testing.Jobs;

/// <summary>
/// 在测试进程里执行后台作业
/// </summary>
public static class JobHarness
{
    /// <summary>
    /// 依次执行所有模块里排队的作业，直到没有可领取的，返回执行的个数
    /// </summary>
    /// <remarks>
    /// 作业执行中提交的新作业也会接着执行
    /// </remarks>
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
