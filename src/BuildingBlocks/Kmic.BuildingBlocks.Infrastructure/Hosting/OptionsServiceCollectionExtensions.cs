using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>
/// 部署配置的注册
/// </summary>
public static class OptionsServiceCollectionExtensions
{
    /// <summary>
    /// 注册一个部署配置类：从配置节绑定，用 DataAnnotations 校验，启动时就校验
    /// </summary>
    /// <remarks>
    /// 任何一项不合法就拒绝启动，错误里列出全部问题。配置在启动时读一次，运行中不变，
    /// 要在运行中调整的参数做成系统设置，不放在这里
    /// </remarks>
    /// <typeparam name="TOptions">配置类，用 sealed class，属性 init</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="section">配置节，框架用 Kmic:{能力}，模块用 Modules:{模块}:{用途}</param>
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
