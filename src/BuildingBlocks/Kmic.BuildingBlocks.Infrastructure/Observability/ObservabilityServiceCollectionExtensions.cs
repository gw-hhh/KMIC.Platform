using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kmic.BuildingBlocks.Infrastructure.Observability;

public static class ObservabilityServiceCollectionExtensions
{
    private static readonly TimeSpan _minSampleInterval = TimeSpan.FromSeconds(5);

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

    /// <summary>注册积压采样后台服务，定期采集消息、作业及定时任务积压指标</summary>
    public static IServiceCollection AddKmicBacklogSampling(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<BacklogSampler>();
        services.AddHostedService(provider => provider.GetRequiredService<BacklogSampler>());
        return services;
    }

    /// <summary>注册模块积压量表</summary>
    public static IServiceCollection AddKmicBacklogGauges<T>(this IServiceCollection services)
        where T : class, IBacklogGauges
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IBacklogGauges, T>());
        return services;
    }
}
