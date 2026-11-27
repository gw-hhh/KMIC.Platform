using Kmic.BuildingBlocks.Infrastructure.Files;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kmic.BuildingBlocks.Hosting.Health;

// 文件存储依赖检查（未启用文件存储时视为健康）
internal sealed class FileStorageDependencyCheck(IServiceProvider services) : IHealthCheck
{
    private const string ProbePrefix = "health-probe/";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (services.GetService(typeof(IFileStorage)) is not IFileStorage storage)
        {
            return HealthCheckResult.Healthy("未启用文件存储");
        }

        // 列举不存在的前缀：能正常返回（包括空结果）即表明存储可连接且凭据有效
        await foreach (var unused in storage.ListAsync(ProbePrefix, cancellationToken))
        {
            break;
        }

        return HealthCheckResult.Healthy("可以访问");
    }
}
