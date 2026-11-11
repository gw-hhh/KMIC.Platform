using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// 把一个业务错误写成 ProblemDetails 响应
/// </summary>
/// <remarks>
/// 经过 IProblemDetailsService 写出，和中间件产生的错误格式一致
/// </remarks>
internal sealed class ProblemResult(Error error) : IResult
{
    // 暂时不可用的错误告诉调用方多久后再试：没有更准确的信息时用一个短的固定值，避免立即重试压垮刚恢复的依赖
    private const int RetryAfterSeconds = 5;

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var details = KmicProblems.From(error, httpContext);
        httpContext.Response.StatusCode = details.Status!.Value;
        if (error.Type == ErrorType.Unavailable)
        {
            httpContext.Response.Headers.RetryAfter = RetryAfterSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        await httpContext.RequestServices.GetRequiredService<IProblemDetailsService>()
            .WriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = details });
    }
}
