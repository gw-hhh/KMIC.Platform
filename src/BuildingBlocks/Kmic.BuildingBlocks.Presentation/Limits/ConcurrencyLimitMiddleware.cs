using System.Threading.RateLimiting;
using Kmic.BuildingBlocks.Presentation.Http;

namespace Kmic.BuildingBlocks.Presentation.Limits;

internal sealed class ConcurrencyLimitMiddleware(RequestDelegate next, ConcurrencyLimiters limiters)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.GetEndpoint()?.Metadata.GetMetadata<ConcurrencyMetadata>() is not { } declared)
        {
            await next(context);
            return;
        }

        var leases = new List<RateLimitLease>(declared.Policies.Count);
        try
        {
            foreach (var policy in declared.Policies)
            {
                var lease = limiters.TryAcquire(policy, context);
                if (!lease.IsAcquired)
                {
                    lease.Dispose();
                    await RejectAsync(context);
                    return;
                }

                leases.Add(lease);
            }

            // 并发许可须持有至请求处理完全结束（含流式响应写入完成）
            await next(context);
        }
        finally
        {
            leases.ForEach(lease => lease.Dispose());
        }
    }

    private static async Task RejectAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        RetryAfter.Set(context.Response);
        await context.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = KmicProblems.From(FrameworkProblems.TooBusy, context),
        });
    }
}
