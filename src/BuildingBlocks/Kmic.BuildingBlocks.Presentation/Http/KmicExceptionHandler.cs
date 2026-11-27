using System.Text.Json;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.AspNetCore.Diagnostics;

namespace Kmic.BuildingBlocks.Presentation.Http;

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

    // 请求体反序列化失败时仅返回字段路径；原始异常消息可能暴露内部类型名，不透传给调用方
    private static IReadOnlyList<FieldError>? FieldErrorsOf(BadHttpRequestException bad) =>
        bad.InnerException is JsonException { Path: { } path }
            ? [new FieldError(ClientPath(path), "Malformed", "格式不正确")]
            : null;

    // System.Text.Json 路径以 $ 开头（如 $.items[2].quantity），移除前缀以对齐字段名；根节点异常时为空字符串
    private static string ClientPath(string path) => path.TrimStart('$').TrimStart('.');
}
