using System.Threading.RateLimiting;
using Kmic.BuildingBlocks.Presentation.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Presentation.Limits;

internal static class RateLimitPolicies
{
    // 已认证请求按用户限流，匿名请求按 IP 限流
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
