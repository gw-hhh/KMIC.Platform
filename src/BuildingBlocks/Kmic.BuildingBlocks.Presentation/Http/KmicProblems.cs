using System.Diagnostics;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.AspNetCore.Mvc;

namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// 生成 ProblemDetails
/// </summary>
/// <remarks>
/// 扩展字段只输出白名单：code、traceId、retryable、errors、currentVersion。
/// Error.Metadata 里的其他键不输出，业务错误不能借附加数据把内部信息带给调用方
/// </remarks>
internal static class KmicProblems
{
    public const string CodeKey = "code";
    public const string TraceIdKey = "traceId";
    public const string RetryableKey = "retryable";
    public const string ErrorsKey = "errors";
    public const string CurrentVersionKey = "currentVersion";
    public const string RetryAfterSecondsKey = "retryAfterSeconds";

    private const string TypePrefix = "urn:kmic:error:";

    /// <summary>
    /// 错误里声明的等待秒数，没有声明或不是正整数时为空
    /// </summary>
    public static int? RetryAfterSecondsOf(Error error) =>
        error.Metadata.TryGetValue(ErrorMetadataKeys.RetryAfterSeconds, out var value) && value is int seconds and > 0 ? seconds : null;

    /// <summary>
    /// 链路追踪 ID，和响应头 X-Trace-Id 相同
    /// </summary>
    /// <remarks>
    /// 没有活动（没有追踪监听器）时退回到请求自己的标识，保证每个响应都有可供报障的编号
    /// </remarks>
    public static string TraceIdOf(HttpContext http) =>
        Activity.Current is { IdFormat: ActivityIdFormat.W3C } activity ? activity.TraceId.ToString() : http.TraceIdentifier;

    public static int StatusOf(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
        ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
        ErrorType.Unauthenticated => StatusCodes.Status401Unauthorized,
        ErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
        ErrorType.PayloadTooLarge => StatusCodes.Status413PayloadTooLarge,
        ErrorType.UnsupportedMediaType => StatusCodes.Status415UnsupportedMediaType,
        ErrorType.Gone => StatusCodes.Status410Gone,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "没有为这种错误类型定义状态码。"),
    };

    // 错误自己声明了 retryable 就用它；没有声明时，暂时不可用和请求过于频繁的错误可以重试
    public static bool IsRetryable(Error error) =>
        error.Metadata.TryGetValue(ErrorMetadataKeys.Retryable, out var value) && value is bool declared
            ? declared
            : error.Type is ErrorType.Unavailable or ErrorType.TooManyRequests;

    public static ProblemDetails From(Error error, HttpContext http)
    {
        var details = Create(error.Code, StatusOf(error.Type), error.Message, error.Detail, http);
        details.Extensions[TraceIdKey] = TraceIdOf(http);
        details.Extensions[RetryableKey] = IsRetryable(error);
        if (error.FieldErrors.Count > 0)
        {
            details.Extensions[ErrorsKey] = error.FieldErrors;
        }

        if (error.Metadata.TryGetValue(ErrorMetadataKeys.CurrentVersion, out var version) && version is not null)
        {
            details.Extensions[CurrentVersionKey] = version;
        }

        if (RetryAfterSecondsOf(error) is { } seconds)
        {
            details.Extensions[RetryAfterSecondsKey] = seconds;
        }

        return details;
    }

    public static ProblemDetails From(FrameworkProblem problem, HttpContext http, IReadOnlyList<FieldError>? fieldErrors = null)
    {
        var details = Create(problem.Code, problem.Status, problem.Title, detail: null, http);
        details.Extensions[TraceIdKey] = TraceIdOf(http);
        details.Extensions[RetryableKey] = problem.Retryable;
        if (fieldErrors is { Count: > 0 })
        {
            details.Extensions[ErrorsKey] = fieldErrors;
        }

        return details;
    }

    private static ProblemDetails Create(string code, int status, string title, string? detail, HttpContext http)
    {
        var details = new ProblemDetails
        {
            Type = TypePrefix + code,
            Title = title,
            Status = status,
            Detail = detail,
            Instance = http.Request.PathBase.Add(http.Request.Path).Value,
        };
        details.Extensions[CodeKey] = code;
        return details;
    }
}
