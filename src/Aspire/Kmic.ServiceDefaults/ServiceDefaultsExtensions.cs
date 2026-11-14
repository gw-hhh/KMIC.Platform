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
    /// 默认输出控制台日志，按数据分类和敏感属性名脱敏；日志级别可通过 Serilog 配置覆盖。
    /// 追踪遵循父级采样决定，无父级时全量采样。配置 OTEL_EXPORTER_OTLP_ENDPOINT 后导出日志、追踪和指标
    /// </remarks>
    public static WebApplicationBuilder AddKmicServiceDefaults(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var identity = new ServiceIdentity(
            builder.Configuration["OTEL_SERVICE_NAME"] is { Length: > 0 } name ? name : "kmic",
            Environment.MachineName,
            builder.Configuration["Kmic:Roles"] ?? "",
            Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown");

        builder.Services.AddKmicRedaction();
        builder.Services.AddOptions<ObservabilityOptions>().Bind(builder.Configuration.GetSection(ObservabilityOptions.Section));
        ConfigureLogging(builder, identity);
        ConfigureTelemetry(builder, identity);
        return builder;
    }

    /// <summary>
    /// 记录请求完成日志
    /// </summary>
    /// <remarks>
    /// 须在错误处理中间件之前注册，以记录 500 响应。摘要模板使用路由，结构化属性仍含实际 RequestPath，不含查询串。
    /// 不记录请求体或响应体，User-Agent 最多保留 200 个字符。健康检查使用 Verbose 级别，默认不输出。
    /// 可通过 Kmic:Observability:RequestLog 关闭
    /// </remarks>
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

        // 清除默认控制台提供程序以免重复输出；保留向其他提供程序转发，供启用 OTLP 后收集日志
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
                    .Destructure.With<ClassifiedDestructuringPolicy>()
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
