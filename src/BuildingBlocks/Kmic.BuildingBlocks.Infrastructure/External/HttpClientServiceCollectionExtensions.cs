using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.External;

/// <summary>外部 HTTP 客户端 DI 注册</summary>
public static class HttpClientServiceCollectionExtensions
{
    /// <summary>注册外部系统类型化客户端，自动配置超时、读请求重试与熔断策略</summary>
    /// <remarks>禁止在命令内直接调用；写请求不自动重试</remarks>
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

        // 守卫位于管道最外层，命令内的调用在进入重试与熔断前即被拦截
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
