using System.Threading.RateLimiting;
using Kmic.BuildingBlocks.Presentation.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Presentation.Limits;

/// <summary>
/// 按时间限流的策略
/// </summary>
internal static class RateLimitPolicies
{
    /// <summary>
    /// 模块路由组的默认策略：已登录的按用户，没有登录的按 IP，各有各的限额
    /// </summary>
    public const string Default = "Default";

    public static void Configure(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = WriteRejectionAsync;
        options.AddPolicy(Default, http =>
        {
            var settings = http.RequestServices.GetRequiredService<IOptions<KmicHttpOptions>>().Value;
            var limit = ClientKey.IsUser(http) ? settings.AuthenticatedRequestsPerMinute : settings.AnonymousRequestsPerMinute;
            return RateLimitPartition.GetFixedWindowLimiter(ClientKey.Of(http), _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            });
        });
    }

    private static async ValueTask WriteRejectionAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var http = context.HttpContext;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            RetryAfter.Set(http.Response, (int)Math.Ceiling(retryAfter.TotalSeconds));
        }

        http.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await http.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            ProblemDetails = KmicProblems.From(FrameworkProblems.TooManyRequests, http),
        });
    }
}
