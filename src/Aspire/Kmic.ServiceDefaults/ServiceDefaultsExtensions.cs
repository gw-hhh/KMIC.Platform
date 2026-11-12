using System.Globalization;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Kmic.ServiceDefaults;

/// <summary>
/// 宿主的可观测性装配
/// </summary>
public static class ServiceDefaultsExtensions
{
    private const string OtlpEndpointKey = "OTEL_EXPORTER_OTLP_ENDPOINT";

    private const string DevelopmentTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";

    private const int MaxUserAgentLength = 200;

    /// <summary>
    /// 配置日志、追踪与指标
    /// </summary>
    /// <remarks>
    /// 日志：开发环境输出可读格式，其他环境输出一行一条的 JSON，不写本地文件；级别默认 Information，
    /// Microsoft.*、System.* 和 EF 的 SQL 命令日志为 Warning，可用 Serilog 配置节覆盖。每条日志带服务、实例、角色、版本，
    /// 名字像密码、令牌的属性一律遮盖。追踪与指标：框架的 Kmic.* 源与 Meter、ASP.NET Core、HttpClient、Npgsql、运行时，
    /// 应用全量记录、不做采样（采样由收集器做尾部采样）；配置了 OTEL_EXPORTER_OTLP_ENDPOINT 才导出
    /// </remarks>
    /// <param name="builder">宿主构建器</param>
    public static WebApplicationBuilder AddKmicServiceDefaults(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var identity = new ServiceIdentity(
            builder.Configuration["OTEL_SERVICE_NAME"] is { Length: > 0 } name ? name : "kmic",
            Environment.MachineName,
            builder.Configuration["Kmic:Roles"] ?? "",
            Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown");

        builder.Services.AddOptions<ObservabilityOptions>().Bind(builder.Configuration.GetSection(ObservabilityOptions.Section));
        ConfigureLogging(builder, identity);
        ConfigureTelemetry(builder, identity);
        return builder;
    }

    /// <summary>
    /// 每个请求记一条摘要日志：方法、路由模板、状态码、耗时、用户、角色、客户端地址
    /// </summary>
    /// <remarks>
    /// 要放在管道的最前面，错误处理之前，这样 500 也能记到。路由模板不含具体 ID，路径和查询串里的内容不进摘要，
    /// 请求头（除截断的 User-Agent）、请求体、响应体都不记，健康检查不记。可用 Kmic:Observability:RequestLog 关闭
    /// </remarks>
    /// <param name="app">应用</param>
    public static WebApplication UseKmicRequestLogging(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (!app.Configuration.GetSection(ObservabilityOptions.Section).GetValue("RequestLog", true))
        {
            return app;
        }

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RoutePattern} responded {StatusCode} in {Elapsed:0.0} ms";
            options.GetLevel = RequestLevel;
            options.EnrichDiagnosticContext = (diagnostics, context) =>
            {
                diagnostics.Set("RoutePattern", (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "(unmatched)");
                diagnostics.Set("ClientIp", context.Connection.RemoteIpAddress?.ToString());

                // 中间件在请求结束时才记日志，访问上下文那时已经建好；匿名请求（含认证失败）身份为空
                var user = context.RequestServices.GetService<IExecutionContext>()?.User;
                diagnostics.Set("UserId", user?.UserId?.Value);
                diagnostics.Set("RoleId", user?.RoleId?.Value);
                diagnostics.Set("UserAgent", Truncate(context.Request.Headers.UserAgent.ToString(), MaxUserAgentLength));
            };
        });
        return app;
    }

    private static void ConfigureLogging(WebApplicationBuilder builder, ServiceIdentity identity)
    {
        var development = builder.Environment.IsDevelopment();

        // 默认的控制台提供程序去掉，不然每条日志输出两遍；同时写给其他日志提供程序：配置了 OTLP 时，OpenTelemetry 的日志提供程序靠这条路收到日志
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog(
            (services, logger) =>
            {
                logger
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                    .ReadFrom.Configuration(builder.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Service", identity.Service)
                    .Enrich.WithProperty("Instance", identity.Instance)
                    .Enrich.WithProperty("Roles", identity.Roles)
                    .Enrich.WithProperty("Version", identity.Version)
                    .Enrich.With<SensitivePropertyMasker>();

                if (development)
                {
                    logger.WriteTo.Console(outputTemplate: DevelopmentTemplate, formatProvider: CultureInfo.InvariantCulture);
                }
                else
                {
                    logger.WriteTo.Console(new RenderedCompactJsonFormatter());
                }
            },
            writeToProviders: true);
    }

    private static void ConfigureTelemetry(WebApplicationBuilder builder, ServiceIdentity identity)
    {
        var telemetry = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(identity.Service, serviceVersion: identity.Version, serviceInstanceId: identity.Instance))
            .WithTracing(tracing => tracing
                .SetSampler(new ParentBasedSampler(new AlwaysOnSampler()))
                .AddSource("Kmic.*", "Npgsql")
                .AddAspNetCoreInstrumentation(options => options.Filter = context => !IsHealthCheck(context.Request.Path))
                .AddHttpClientInstrumentation())
            .WithMetrics(metrics => metrics
                .AddMeter("Kmic.*", "Npgsql")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation());

        if (!string.IsNullOrWhiteSpace(builder.Configuration[OtlpEndpointKey]))
        {
            telemetry.WithLogging(
                logging => { },
                options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                });
            telemetry.UseOtlpExporter();
        }
    }

    private static LogEventLevel RequestLevel(HttpContext context, double elapsed, Exception? exception) =>
        (IsHealthCheck(context.Request.Path), exception, context.Response.StatusCode) switch
        {
            (true, _, _) => LogEventLevel.Verbose,
            (_, not null, _) => LogEventLevel.Error,
            (_, _, >= StatusCodes.Status500InternalServerError) => LogEventLevel.Error,
            _ => LogEventLevel.Information,
        };

    private static bool IsHealthCheck(PathString path) => path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);

    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length];

    private sealed record ServiceIdentity(string Service, string Instance, string Roles, string Version);
}
