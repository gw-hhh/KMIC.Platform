using System.Diagnostics.Metrics;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog.Core;

namespace Kmic.ServiceDefaults.Tests;

/// <summary>
/// 套用了默认配置的最小应用：几个端点、内存里的日志接收器和导出器
/// </summary>
internal sealed class TestApp : IAsyncDisposable
{
    private readonly WebApplication _app;

    private TestApp(WebApplication app, CollectingSink logs, List<System.Diagnostics.Activity> spans, List<Metric> metrics)
    {
        _app = app;
        Logs = logs;
        Spans = spans;
        Metrics = metrics;
        Client = app.GetTestClient();
    }

    public CollectingSink Logs { get; }

    public List<System.Diagnostics.Activity> Spans { get; }

    public List<Metric> Metrics { get; }

    public HttpClient Client { get; }

    public IServiceProvider Services => _app.Services;

    public static async Task<TestApp> StartAsync(string environment = "Testing", Dictionary<string, string?>? configuration = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal) { ["Kmic:Roles"] = "Api" });
        if (configuration is not null)
        {
            builder.Configuration.AddInMemoryCollection(configuration);
        }

        var logs = new CollectingSink();
        var spans = new List<System.Diagnostics.Activity>();
        var metrics = new List<Metric>();
        builder.Services.AddSingleton<ILogEventSink>(logs);
        builder.AddKmicServiceDefaults();
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddInMemoryExporter(spans));
        builder.Services.ConfigureOpenTelemetryMeterProvider(metering => metering.AddInMemoryExporter(metrics));

        var app = builder.Build();
        app.UseKmicRequestLogging();
        app.MapGet("/orders/{id}", (string id) => Results.Ok(new { id }));
        app.MapGet("/boom", () => Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
        app.MapGet("/crash", void () => throw new InvalidOperationException("crash"));
        app.MapGet("/health/live", () => Results.Ok());
        await app.StartAsync(TestContext.Current.CancellationToken);
        return new TestApp(app, logs, spans, metrics);
    }

    public void ForceFlush()
    {
        _app.Services.GetRequiredService<TracerProvider>().ForceFlush();
        _app.Services.GetRequiredService<MeterProvider>().ForceFlush();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }

    // 请求日志在响应送出之后才记，测试客户端可能先拿到响应：等到指定数量的摘要出现
    public Task WaitForRequestSummariesAsync(int count) => Eventually.UntilAsync(
        _ => Task.FromResult(Logs.Events.Count(e => e.IsRequestSummary()) >= count),
        $"等到 {count} 条请求摘要",
        ct: TestContext.Current.CancellationToken);

    public static Counter<long> NewCounter(string meterName, string name) => new Meter(meterName).CreateCounter<long>(name);
}
