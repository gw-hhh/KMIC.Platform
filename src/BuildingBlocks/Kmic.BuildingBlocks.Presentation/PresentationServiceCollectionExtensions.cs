using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.BuildingBlocks.Presentation.Limits;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Timeouts;
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
    /// 放在路由和认证之后、授权之前：限流和并发限制要按端点的声明和登录的用户判断，授权失败的请求也要计数
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
