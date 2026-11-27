namespace Kmic.Operations.Application.ClientErrors;

/// <summary>前端上报的单条错误（含未知字段时拒绝整次请求）。</summary>
/// <param name="Type">错误类型</param>
/// <param name="Message">错误消息</param>
/// <param name="Stack">堆栈</param>
/// <param name="RouteName">出错时的路由名</param>
/// <param name="AppVersion">前端版本</param>
/// <param name="TraceId">相关接口响应的追踪 ID</param>
/// <param name="OccurredAt">浏览器中的错误发生时间</param>
public sealed record ClientErrorReport(
    string Type,
    string Message,
    string? Stack,
    string? RouteName,
    string? AppVersion,
    string? TraceId,
    DateTimeOffset? OccurredAt);
