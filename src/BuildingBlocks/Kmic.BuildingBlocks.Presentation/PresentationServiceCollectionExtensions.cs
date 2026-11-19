using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.BuildingBlocks.Presentation.Limits;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Presentation;

/// <summary>
/// 接口层的服务注册与中间件
/// </summary>
public static class PresentationServiceCollectionExtensions
{
    /// <summary>
    /// 注册接口层：JSON 选项、ProblemDetails、请求解析错误的转换、限流与并发限制、请求超时、端点声明的启动校验
    /// </summary>
    public static IServiceCollection AddKmicPresentation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // 部署配置在启动时就校验，不合法的值拒绝启动
        services.AddOptions<KmicHttpOptions>()
            .BindConfiguration(KmicHttpOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // 只采信可信代理传来的客户端地址和协议；没配置可信代理时不处理转发头
        services.AddOptions<ForwardedHeadersOptions>().Configure<IOptions<KmicHttpOptions>>((forwarded, http) =>
        {
            forwarded.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            forwarded.ForwardLimit = 1;
            forwarded.KnownProxies.Clear();
            forwarded.KnownIPNetworks.Clear();
            foreach (var network in http.Value.TrustedProxies)
            {
                forwarded.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
            }
        });

        // 请求体默认不超过 1 MB，匿名接口也不能被大请求体拖垮；上传等端点在请求里自己放宽
        services.AddOptions<KestrelServerOptions>().Configure<IOptions<KmicHttpOptions>>((kestrel, http) =>
        {
            kestrel.Limits.MaxRequestBodySize = http.Value.MaxRequestBodyBytes;
            kestrel.AddServerHeader = false;
        });

        // ASP.NET Core 的默认值比框架约定宽松，要逐项覆盖
        services.ConfigureHttpJsonOptions(options => KmicJsonOptions.Configure(options.SerializerOptions));

        // 请求体解析失败时抛 BadHttpRequestException，才能带着字段路径转成 400
        services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

        // 只保留我们的写出方：任何错误的输出格式都一样
        services.AddProblemDetails();
        services.RemoveAll<IProblemDetailsWriter>();
        services.AddSingleton<IProblemDetailsWriter, KmicProblemDetailsWriter>();
        services.AddExceptionHandler<KmicExceptionHandler>();

        services.AddRateLimiter(RateLimitPolicies.Configure);
        services.AddSingleton<ConcurrencyLimiters>();

        // 请求超时后取消请求的令牌，处理器应当在等待中响应取消；没来得及写响应的返回 504
        services.AddRequestTimeouts();
        services.AddOptions<RequestTimeoutOptions>().Configure<IOptions<KmicHttpOptions>>((timeouts, http) =>
            timeouts.DefaultPolicy = new RequestTimeoutPolicy
            {
                Timeout = http.Value.RequestTimeout,
                TimeoutStatusCode = StatusCodes.Status504GatewayTimeout,
            });

        services.AddTransient<IStartupFilter, EndpointValidationFilter>();
        return services;
    }

    /// <summary>
    /// 最外层的错误处理：X-Trace-Id、未处理异常转 500、没有内容的 4xx 转 ProblemDetails
    /// </summary>
    /// <summary>
    /// 只采信可信代理传来的客户端地址和协议，要放在管道最前面
    /// </summary>
    public static IApplicationBuilder UseKmicForwardedHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseForwardedHeaders();
    }

    /// <summary>
    /// 给所有响应加安全响应头，要放在错误处理之前，出错的响应也带
    /// </summary>
    public static IApplicationBuilder UseKmicSecurityHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }

    public static IApplicationBuilder UseKmicErrorHandling(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<TraceIdMiddleware>();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        return app;
    }

    /// <summary>
    /// 限流、并发限制和请求超时
    /// </summary>
    /// <remarks>
    /// 放在路由和认证之后、授权之前
    /// </remarks>
    public static IApplicationBuilder UseKmicLimits(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseRateLimiter();
        app.UseMiddleware<ConcurrencyLimitMiddleware>();
        app.UseRequestTimeouts();
        return app;
    }
}
