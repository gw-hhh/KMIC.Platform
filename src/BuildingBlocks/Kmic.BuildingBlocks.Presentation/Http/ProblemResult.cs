using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Presentation.Limits;

namespace Kmic.BuildingBlocks.Presentation.Http;

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
            // 未认证响应需附带 WWW-Authenticate 认证方案，便于 Bearer 客户端决定是否刷新令牌
            httpContext.Response.Headers.WWWAuthenticate = "Bearer";
        }

        await httpContext.RequestServices.GetRequiredService<IProblemDetailsService>()
            .WriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = details });
    }
}
