using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kmic.BuildingBlocks.Infrastructure.Observability;

/// <summary>
/// 数据库侧观测的服务注册
/// </summary>
public static class ObservabilityServiceCollectionExtensions
{
    private static readonly TimeSpan _minSampleInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 注册配置与慢查询拦截器，数据库上下文创建时挂上，由 AddKmicPersistence 调用
    /// </summary>
    /// <param name="services">服务集合</param>
    internal static IServiceCollection AddKmicSlowQueryLog(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<DatabaseDiagnosticsOptions>()
            .Validate(o => o.SlowQueryMs > 0, $"{DatabaseDiagnosticsOptions.Section}:SlowQueryMs 必须大于 0。")
            .Validate(o => o.BacklogSampleInterval >= _minSampleInterval, $"{DatabaseDiagnosticsOptions.Section}:BacklogSampleInterval 不能小于 5 秒。")
            .ValidateOnStart();
        services.TryAddSingleton<SlowQueryInterceptor>();
        return services;
    }

    /// <summary>
    /// 注册积压采样：每个实例定期统计消息、作业与定时任务的积压，积压量表读取采样结果
    /// </summary>
    /// <param name="services">服务集合</param>
    public static IServiceCollection AddKmicBacklogSampling(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<BacklogSampler>();
        services.AddHostedService(provider => provider.GetRequiredService<BacklogSampler>());
        return services;
    }
}
