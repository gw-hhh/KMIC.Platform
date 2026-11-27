using System.Collections.Frozen;
using Kmic.BuildingBlocks.Domain.Errors;

namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>框架层 HTTP 错误目录，涵盖认证、请求解析、限流及未处理异常</summary>
public static class FrameworkProblems
{
    /// <summary>未登录或登录已失效</summary>
    public static readonly FrameworkProblem Unauthenticated = new("Common.Unauthenticated", StatusCodes.Status401Unauthorized, "请先登录");

    /// <summary>当前角色不可用，仅 me 与 switch-role 允许访问（Identity 模块不得重复声明）</summary>
    public static readonly FrameworkProblem CurrentRoleUnavailable = new("Identity.CurrentRoleUnavailable", StatusCodes.Status403Forbidden, "当前角色已不可用，请重新选择角色");

    /// <summary>请求体无法解析、类型不符、含未知字段或路由参数格式错误</summary>
    public static readonly FrameworkProblem MalformedRequest = new("Common.MalformedRequest", StatusCodes.Status400BadRequest, "请求格式不正确");

    public static readonly FrameworkProblem PayloadTooLarge = new("Common.PayloadTooLarge", StatusCodes.Status413PayloadTooLarge, "请求内容过大");

    public static readonly FrameworkProblem UnsupportedMediaType = new("Common.UnsupportedMediaType", StatusCodes.Status415UnsupportedMediaType, "不支持该内容类型，请使用 application/json");

    /// <summary>触发时间窗口限流</summary>
    public static readonly FrameworkProblem TooManyRequests = new("Common.TooManyRequests", StatusCodes.Status429TooManyRequests, "请求过于频繁，请稍后再试", Retryable: true);

    /// <summary>重型接口并发已达上限</summary>
    public static readonly FrameworkProblem TooBusy = new("Common.TooBusy", StatusCodes.Status429TooManyRequests, "系统繁忙，请稍后再试", Retryable: true);

    public static readonly FrameworkProblem InternalError = new("Common.InternalError", StatusCodes.Status500InternalServerError, "系统异常，请稍后再试");

    public static readonly FrameworkProblem RequestTimeout = new("Common.RequestTimeout", StatusCodes.Status504GatewayTimeout, "请求超时，请缩小范围后重试");

    public static readonly FrameworkProblem RouteNotFound = new("Common.RouteNotFound", StatusCodes.Status404NotFound, "接口不存在");

    public static readonly FrameworkProblem MethodNotAllowed = new("Common.MethodNotAllowed", StatusCodes.Status405MethodNotAllowed, "不支持该请求方法");

    // 其他未单独归类的 4xx/5xx 状态码
    private static readonly FrameworkProblem _other = new("Common.HttpError", StatusCodes.Status400BadRequest, "请求失败");

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

    public static IReadOnlyCollection<FrameworkProblem> All { get; } =
        [Unauthenticated, CurrentRoleUnavailable, MalformedRequest, PayloadTooLarge, UnsupportedMediaType, TooManyRequests, TooBusy, InternalError, RequestTimeout, RouteNotFound, MethodNotAllowed, _other];

    internal static FrameworkProblem ForStatus(int status) =>
        _byStatus.TryGetValue(status, out var problem) ? problem : _other with { Status = status, Retryable = status is 502 or 503 };
}
