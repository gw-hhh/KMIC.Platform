using System.Text.Json;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.AspNetCore.Diagnostics;

namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// 把请求本身的问题转成 4xx：请求体无法解析、请求体太大等
/// </summary>
/// <remarks>
/// 其他异常不处理，交给异常处理中间件按未处理异常写成 500
/// </remarks>
internal sealed class KmicExceptionHandler(IProblemDetailsService problems) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (exception is not BadHttpRequestException bad)
        {
            return false;
        }

        var details = bad.StatusCode switch
        {
            StatusCodes.Status413PayloadTooLarge => KmicProblems.From(FrameworkProblems.PayloadTooLarge, httpContext),
            StatusCodes.Status415UnsupportedMediaType => KmicProblems.From(FrameworkProblems.UnsupportedMediaType, httpContext),
            StatusCodes.Status400BadRequest => KmicProblems.From(FrameworkProblems.MalformedRequest, httpContext, FieldErrorsOf(bad)),
            _ => KmicProblems.From(FrameworkProblems.ForStatus(bad.StatusCode), httpContext),
        };

        httpContext.Response.StatusCode = details.Status!.Value;
        await problems.WriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = details, Exception = exception });
        return true;
    }

    // 解析请求体出错时给出字段路径。异常消息是英文且可能带内部类型名，不给调用方，也不去解析它
    private static IReadOnlyList<FieldError>? FieldErrorsOf(BadHttpRequestException bad) =>
        bad.InnerException is JsonException { Path: { } path }
            ? [new FieldError(ClientPath(path), "Malformed", "格式不正确")]
            : null;

    // System.Text.Json 的路径以 $ 开头：$.items[2].quantity 去掉前缀，整个请求体有问题时是空字符串
    private static string ClientPath(string path) => path.TrimStart('$').TrimStart('.');
}
