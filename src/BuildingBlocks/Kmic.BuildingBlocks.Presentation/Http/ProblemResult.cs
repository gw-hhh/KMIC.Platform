using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Presentation.Limits;

namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// 把一个业务错误写成 ProblemDetails 响应
/// </summary>
/// <remarks>
/// 经过 IProblemDetailsService 写出，和中间件产生的错误格式一致
/// </remarks>
internal sealed class ProblemResult(Error error) : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var details = KmicProblems.From(error, httpContext);
        httpContext.Response.StatusCode = details.Status!.Value;
        if (error.Type == ErrorType.Unavailable)
        {
            RetryAfter.Set(httpContext.Response);
        }
        else if (error.Type == ErrorType.TooManyRequests)
        {
            RetryAfter.Set(httpContext.Response, KmicProblems.RetryAfterSecondsOf(error) ?? RetryAfter.DefaultSeconds);
        }

        if (error.Type == ErrorType.Unauthenticated)
        {
            // 没有凭据或凭据无效的响应要说明认证方案，Bearer 令牌的客户端据此判断是否去刷新
            httpContext.Response.Headers.WWWAuthenticate = "Bearer";
        }

        await httpContext.RequestServices.GetRequiredService<IProblemDetailsService>()
            .WriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = details });
    }
}
