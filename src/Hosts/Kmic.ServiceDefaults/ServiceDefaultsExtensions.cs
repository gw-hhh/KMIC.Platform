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

public static class ServiceDefaultsExtensions
{
    private const string OtlpEndpointKey = "OTEL_EXPORTER_OTLP_ENDPOINT";

    private const string DevelopmentTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";

    private const int MaxUserAgentLength = 200;

    /// <summary>配置结构化日志、追踪与指标（存在 OTEL_EXPORTER_OTLP_ENDPOINT 时开启 OTLP 导出）</summary>
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

    /// <summary>注册 HTTP 请求摘要日志（仅记录路由模板）；须置于全局异常处理中间件之前以确保 500 异常也能记录</summary>
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

                // 请求日志在响应结束后写入，此时用户身份上下文已构建完成；匿名请求或认证失败时用户身份为空
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

        // 清除默认控制台提供程序以避免重复输出，并保留向 ILoggerProvider 转发以支持 OTLP 日志收集
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
                    .Enrich.With<SensitivePropertyMasker>()
                    .Enrich.With<RequestPathRemover>();

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
