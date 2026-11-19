namespace Kmic.Operations.Application.ClientErrors;

/// <summary>
/// 前端上报的一条错误
/// </summary>
/// <remarks>
/// 只收下面这些字段，有未知字段整次请求被拒；不要上报请求体、表单值或 URL 查询参数
/// </remarks>
/// <param name="Type">错误类型，如 TypeError、ApiError，最多 100 个字符</param>
/// <param name="Message">错误消息，最多 1000 个字符</param>
/// <param name="Stack">堆栈，生产环境是压缩后的位置，最多 8000 个字符</param>
/// <param name="RouteName">出错时的路由名，不是带参数的地址，最多 200 个字符</param>
/// <param name="AppVersion">前端版本，最多 50 个字符</param>
/// <param name="TraceId">相关接口响应的追踪 ID，用于关联后端调用链，须为 32 位小写十六进制</param>
/// <param name="OccurredAt">浏览器中的错误发生时间</param>
public sealed record ClientErrorReport(
    string Type,
    string Message,
    string? Stack,
    string? RouteName,
    string? AppVersion,
    string? TraceId,
    DateTimeOffset? OccurredAt);
