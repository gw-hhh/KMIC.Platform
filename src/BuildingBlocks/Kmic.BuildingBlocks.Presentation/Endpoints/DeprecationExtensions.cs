using System.Globalization;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 端点的废弃声明
/// </summary>
public static class DeprecationExtensions
{
    /// <summary>
    /// 标记端点已废弃：响应带 Deprecation 和 Sunset 头，OpenAPI 文档里标为废弃
    /// </summary>
    /// <remarks>
    /// 破坏性变化只升级发生变化的那个端点：新版本的端点放在新的版本路由组里，旧端点标上废弃并保留到下线日期
    /// </remarks>
    /// <param name="builder">端点</param>
    /// <param name="since">从哪天起废弃，写入 Deprecation 头</param>
    /// <param name="sunset">从哪天起不再提供，写入 Sunset 头，不能早于废弃日期</param>
    public static TBuilder Deprecated<TBuilder>(this TBuilder builder, DateOnly since, DateOnly sunset)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentOutOfRangeException.ThrowIfLessThan(sunset, since);

        var deprecation = new DeprecationMetadata(since, sunset);
        var headers = (Deprecation: $"@{Utc(since).ToUnixTimeSeconds()}", Sunset: Utc(sunset).ToString("r", CultureInfo.InvariantCulture));
        return builder
            .WithMetadata(deprecation)
            .AddEndpointFilter(async (context, next) =>
            {
                // Deprecation 是 RFC 9745 的日期值（@ 加 Unix 秒数），Sunset 是 RFC 8594 的 HTTP 日期
                context.HttpContext.Response.Headers["Deprecation"] = headers.Deprecation;
                context.HttpContext.Response.Headers["Sunset"] = headers.Sunset;
                return await next(context);
            });
    }

    private static DateTimeOffset Utc(DateOnly date) => new(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
}
