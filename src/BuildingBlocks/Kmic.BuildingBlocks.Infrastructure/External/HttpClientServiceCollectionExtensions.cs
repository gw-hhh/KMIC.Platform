using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.External;

/// <summary>
/// 外部系统类型化客户端的注册
/// </summary>
public static class HttpClientServiceCollectionExtensions
{
    /// <summary>
    /// 注册一个外部系统的类型化客户端：命令里不能调用、总超时与单次超时、读请求重试、熔断
    /// </summary>
    /// <remarks>
    /// 每个外部系统一个客户端，接口在 Application，实现在 Infrastructure。写请求不自动重试，
    /// 对方支持幂等键时由调用方带上键后再自己重试
    /// </remarks>
    /// <typeparam name="TClient">客户端接口</typeparam>
    /// <typeparam name="TImplementation">客户端实现，构造函数接收 HttpClient</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="name">客户端名称</param>
    /// <param name="configure">超时与重试</param>
    public static IHttpClientBuilder AddKmicHttpClient<TClient, TImplementation>(
        this IServiceCollection services, string name, Action<ExternalClientOptions>? configure = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var options = new ExternalClientOptions();
        configure?.Invoke(options);

        var builder = services.AddHttpClient<TClient, TImplementation>(name);

        // 守卫放在最外层：命令里的调用在进入韧性管道之前就被拒绝，不会触发重试和熔断
        builder.AddHttpMessageHandler(() => new ExternalCallGuardHandler());
        builder.AddStandardResilienceHandler(resilience =>
        {
            resilience.TotalRequestTimeout.Timeout = options.TotalTimeout;
            resilience.AttemptTimeout.Timeout = options.AttemptTimeout;
            resilience.CircuitBreaker.SamplingDuration = options.AttemptTimeout * 2;
            var handled = resilience.Retry.ShouldHandle;
            resilience.Retry.ShouldHandle = async args =>
            {
                var method = args.Outcome.Result?.RequestMessage?.Method;
                var isRead = method is null || method == HttpMethod.Get || method == HttpMethod.Head;
                return options.RetryReads && isRead && await handled(args);
            };
        });
        return builder;
    }
}
