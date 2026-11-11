using System.Threading.RateLimiting;
using Kmic.BuildingBlocks.Presentation.Http;

namespace Kmic.BuildingBlocks.Presentation.Limits;

/// <summary>
/// 按端点声明的并发限制放行请求
/// </summary>
/// <remarks>
/// 要放在路由和认证之后：按端点的元数据判断，重型接口按用户限制时要知道用户是谁
/// </remarks>
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

            // 许可一直持有到处理完，包括流式响应写完
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
