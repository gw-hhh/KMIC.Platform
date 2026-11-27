using System.Globalization;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

public static class DeprecationExtensions
{
    /// <summary>标记端点已废弃：附加 Deprecation 与 Sunset 响应头，并在 OpenAPI 中标为废弃</summary>
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
