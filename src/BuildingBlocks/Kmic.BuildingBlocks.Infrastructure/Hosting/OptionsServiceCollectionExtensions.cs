using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>部署配置 DI 注册</summary>
public static class OptionsServiceCollectionExtensions
{
    /// <summary>注册部署配置类，绑定配置节并在启动时校验（需运行时动态调整的参数请使用系统设置）</summary>
    public static OptionsBuilder<TOptions> AddKmicOptions<TOptions>(this IServiceCollection services, string section)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(section);

        return services.AddOptions<TOptions>()
            .BindConfiguration(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
