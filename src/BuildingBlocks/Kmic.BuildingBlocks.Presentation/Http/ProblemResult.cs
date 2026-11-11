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

        await httpContext.RequestServices.GetRequiredService<IProblemDetailsService>()
            .WriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = details });
    }
}
