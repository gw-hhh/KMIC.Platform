using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kmic.BuildingBlocks.Presentation;

/// <summary>
/// 接口层的服务注册与中间件
/// </summary>
public static class PresentationServiceCollectionExtensions
{
    /// <summary>
    /// 注册接口层：JSON 选项、ProblemDetails、请求解析错误的转换、端点声明的启动校验
    /// </summary>
    public static IServiceCollection AddKmicPresentation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // ASP.NET Core 的默认值比框架约定宽松，要逐项覆盖
        services.ConfigureHttpJsonOptions(options => KmicJsonOptions.Configure(options.SerializerOptions));

        // 请求体解析失败时抛 BadHttpRequestException，才能带着字段路径转成 400
        services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

        // 只保留我们的写出方：任何错误的输出格式都一样
        services.AddProblemDetails();
        services.RemoveAll<IProblemDetailsWriter>();
        services.AddSingleton<IProblemDetailsWriter, KmicProblemDetailsWriter>();
        services.AddExceptionHandler<KmicExceptionHandler>();

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
}
