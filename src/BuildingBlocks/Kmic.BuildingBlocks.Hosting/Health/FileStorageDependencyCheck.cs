using Kmic.BuildingBlocks.Infrastructure.Files;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kmic.BuildingBlocks.Hosting.Health;

/// <summary>
/// 文件存储（本地目录或对象存储），没有登记文件存储的部署跳过
/// </summary>
internal sealed class FileStorageDependencyCheck(IServiceProvider services) : IHealthCheck
{
    private const string ProbePrefix = "health-probe/";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (services.GetService(typeof(IFileStorage)) is not IFileStorage storage)
        {
            return HealthCheckResult.Healthy("没有启用文件存储");
        }

        // 列举一个不存在的前缀：能返回（哪怕是空）就说明存储可以连接、凭据有效
        await foreach (var unused in storage.ListAsync(ProbePrefix, cancellationToken))
        {
            break;
        }

        return HealthCheckResult.Healthy("可以访问");
    }
}
