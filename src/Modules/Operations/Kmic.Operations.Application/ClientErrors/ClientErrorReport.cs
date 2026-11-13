namespace Kmic.Operations.Application.ClientErrors;

/// <summary>
/// 前端上报的一条错误
/// </summary>
/// <remarks>
/// 字段是白名单：不接收请求体、表单值、地址里的查询参数。未知字段整个请求被拒绝
/// </remarks>
/// <param name="Type">错误类型，如 TypeError、ApiError，最多 100 个字符</param>
/// <param name="Message">错误消息，最多 1000 个字符</param>
/// <param name="Stack">堆栈，生产环境是压缩后的位置，最多 8000 个字符</param>
/// <param name="RouteName">出错时的路由名，不是带参数的地址，最多 200 个字符</param>
/// <param name="AppVersion">前端版本，最多 50 个字符</param>
/// <param name="TraceId">相关接口响应里的追踪 ID，用来和后端的追踪关联，32 位十六进制</param>
/// <param name="OccurredAt">在浏览器上发生的时间</param>
public sealed record ClientErrorReport(
    string Type,
    string Message,
    string? Stack,
    string? RouteName,
    string? AppVersion,
    string? TraceId,
    DateTimeOffset? OccurredAt);
