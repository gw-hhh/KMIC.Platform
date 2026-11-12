using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kmic.BuildingBlocks.Hosting.Health;

/// <summary>
/// 依赖状态端点：数据库、监听连接、调度器、文件存储等全部依赖的详细状态
/// </summary>
/// <remarks>
/// 给运维和监控用，只接受直接来自内网的请求，其他来源一律 404，不暴露这个端点存在。
/// 响应里只有检查自己写的说明，异常内容（可能带地址、账号）不输出；整体不可用返回 503，降级仍是 200
/// </remarks>
internal static class DependencyEndpoint
{
    public const string Path = "/health/dependencies";

    public static IEndpointConventionBuilder Map(IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.MapGet(Path, async (HttpContext http, HealthCheckService health, CancellationToken ct) =>
        {
            if (!InternalRequests.IsInternal(http))
            {
                return Results.NotFound();
            }

            var report = await health.CheckHealthAsync(registration => registration.Tags.Contains(DependencyHealth.Tag), ct);
            return Results.Json(Describe(report), statusCode: report.Status == HealthStatus.Unhealthy ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status200OK);
        }).ExcludeFromDescription();
    }

    private static DependencyReport Describe(HealthReport report) => new(
        report.Status.ToString(),
        report.TotalDuration.TotalMilliseconds,
        [.. report.Entries
            .OrderBy(e => e.Key, StringComparer.Ordinal)
            .Select(e => new DependencyEntry(e.Key, e.Value.Status.ToString(), Explain(e.Value), e.Value.Duration.TotalMilliseconds))]);

    private static string Explain(HealthReportEntry entry) => entry.Exception switch
    {
        null => entry.Description ?? "",
        OperationCanceledException or TimeoutException => "超时",
        _ => "检查失败",
    };
}
