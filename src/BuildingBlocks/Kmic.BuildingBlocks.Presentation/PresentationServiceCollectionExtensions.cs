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

/// <summary>接口层服务注册与中间件扩展</summary>
public static class PresentationServiceCollectionExtensions
{
    /// <summary>注册接口层基础服务（JSON、异常处理、限流、超时与端点校验）</summary>
    public static IServiceCollection AddKmicPresentation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // 启动时校验部署配置，存在非法值则拒绝启动
        services.AddOptions<KmicHttpOptions>()
            .BindConfiguration(KmicHttpOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // 仅采信可信代理传递的客户端地址与协议；未配置可信代理时不处理转发头
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

        // 请求体大小默认限制为 1 MB，防止大请求体耗尽匿名接口资源；上传等端点按需自行放宽
        services.AddOptions<KestrelServerOptions>().Configure<IOptions<KmicHttpOptions>>((kestrel, http) =>
        {
            kestrel.Limits.MaxRequestBodySize = http.Value.MaxRequestBodyBytes;
            kestrel.AddServerHeader = false;
        });

        // ASP.NET Core 默认选项较框架约定宽松，需逐项显式覆盖
        services.ConfigureHttpJsonOptions(options => KmicJsonOptions.Configure(options.SerializerOptions));

        // 请求体解析失败时抛出 BadHttpRequestException，以便提取字段路径并转换为 400 响应
        services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

        // 仅保留自定义 ProblemDetails 写出器，确保所有错误响应格式一致
        services.AddProblemDetails();
        services.RemoveAll<IProblemDetailsWriter>();
        services.AddSingleton<IProblemDetailsWriter, KmicProblemDetailsWriter>();
        services.AddExceptionHandler<KmicExceptionHandler>();

        services.AddRateLimiter(RateLimitPolicies.Configure);
        services.AddSingleton<ConcurrencyLimiters>();

        // 请求超时时触发取消令牌，处理器应在等待过程中响应取消；未及写入响应时返回 504
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

    /// <summary>采信可信代理传递的客户端地址与协议（须置于管道最前端）</summary>
    public static IApplicationBuilder UseKmicForwardedHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseForwardedHeaders();
    }

    /// <summary>写入安全响应头（须置于错误处理中间件之前）</summary>
    public static IApplicationBuilder UseKmicSecurityHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }

    /// <summary>最外层错误处理：注入 X-Trace-Id、兜底未处理异常并统一 4xx 错误响应</summary>
    public static IApplicationBuilder UseKmicErrorHandling(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<TraceIdMiddleware>();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        return app;
    }

    /// <summary>启用限流、并发与超时控制（须置于路由与认证之后、授权之前）</summary>
    public static IApplicationBuilder UseKmicLimits(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseRateLimiter();
        app.UseMiddleware<ConcurrencyLimitMiddleware>();
        app.UseRequestTimeouts();
        return app;
    }
}
