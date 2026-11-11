using System.Collections.Frozen;
using Kmic.BuildingBlocks.Domain.Errors;

namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// 框架层的 HTTP 错误目录
/// </summary>
/// <remarks>
/// 业务错误只来自 Result；这里的错误来自认证、请求解析、限流和未处理的异常，前端同样按 code 处理
/// </remarks>
public static class FrameworkProblems
{
    /// <summary>
    /// 没有登录，或登录已失效
    /// </summary>
    public static readonly FrameworkProblem Unauthenticated = new("Common.Unauthenticated", StatusCodes.Status401Unauthorized, "请先登录");

    /// <summary>
    /// 请求体无法解析、类型不符、有未知字段，或路由参数格式错误
    /// </summary>
    public static readonly FrameworkProblem MalformedRequest = new("Common.MalformedRequest", StatusCodes.Status400BadRequest, "请求的格式不正确");

    /// <summary>
    /// 请求体超过上限
    /// </summary>
    public static readonly FrameworkProblem PayloadTooLarge = new("Common.PayloadTooLarge", StatusCodes.Status413PayloadTooLarge, "请求的内容太大");

    /// <summary>
    /// 请求的内容类型不是 application/json
    /// </summary>
    public static readonly FrameworkProblem UnsupportedMediaType = new("Common.UnsupportedMediaType", StatusCodes.Status415UnsupportedMediaType, "不支持这种内容类型，请使用 application/json");

    /// <summary>
    /// 按时间限流
    /// </summary>
    public static readonly FrameworkProblem TooManyRequests = new("Common.TooManyRequests", StatusCodes.Status429TooManyRequests, "请求太频繁，请稍后再试", Retryable: true);

    /// <summary>
    /// 重型接口的并发已满
    /// </summary>
    public static readonly FrameworkProblem TooBusy = new("Common.TooBusy", StatusCodes.Status429TooManyRequests, "系统正忙，请稍后再试", Retryable: true);

    /// <summary>
    /// 未处理的异常
    /// </summary>
    /// <remarks>
    /// 不可重试：未知错误下无法判断是否已经生效
    /// </remarks>
    public static readonly FrameworkProblem InternalError = new("Common.InternalError", StatusCodes.Status500InternalServerError, "系统出错了，请稍后再试");

    /// <summary>
    /// 请求处理超过了请求超时
    /// </summary>
    /// <remarks>
    /// 不可重试：无法判断操作是否已经生效。要处理这么久的操作应该做成后台作业
    /// </remarks>
    public static readonly FrameworkProblem RequestTimeout = new("Common.RequestTimeout", StatusCodes.Status504GatewayTimeout, "处理时间太长，请缩小范围后再试");

    /// <summary>
    /// 路由不存在
    /// </summary>
    public static readonly FrameworkProblem RouteNotFound = new("Common.RouteNotFound", StatusCodes.Status404NotFound, "接口不存在");

    /// <summary>
    /// 路由存在，但不支持这种请求方法
    /// </summary>
    public static readonly FrameworkProblem MethodNotAllowed = new("Common.MethodNotAllowed", StatusCodes.Status405MethodNotAllowed, "不支持这种请求方法");

    /// <summary>
    /// 没有列在上面的其他 4xx、5xx 状态码
    /// </summary>
    private static readonly FrameworkProblem _other = new("Common.HttpError", StatusCodes.Status400BadRequest, "请求没有成功");

    private static readonly FrozenDictionary<int, FrameworkProblem> _byStatus = new[]
    {
        Unauthenticated,
        MalformedRequest,
        PayloadTooLarge,
        UnsupportedMediaType,
        TooManyRequests,
        InternalError,
        RequestTimeout,
        RouteNotFound,
        MethodNotAllowed,
        new FrameworkProblem(CommonErrors.MissingPermission.Code, StatusCodes.Status403Forbidden, CommonErrors.MissingPermission.Message),
    }.ToFrozenDictionary(p => p.Status);

    /// <summary>
    /// 全部框架层错误，给错误码目录导出用
    /// </summary>
    public static IReadOnlyCollection<FrameworkProblem> All { get; } =
        [Unauthenticated, MalformedRequest, PayloadTooLarge, UnsupportedMediaType, TooManyRequests, TooBusy, InternalError, RequestTimeout, RouteNotFound, MethodNotAllowed, _other];

    /// <summary>
    /// 状态码对应的错误：中间件只设置了状态码、没有写内容时使用
    /// </summary>
    internal static FrameworkProblem ForStatus(int status) =>
        _byStatus.TryGetValue(status, out var problem) ? problem : _other with { Status = status, Retryable = status is 502 or 503 };
}
