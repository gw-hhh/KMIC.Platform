namespace Kmic.Notifications.Presentation.Captured;

/// <summary>
/// 查询捕获内容的参数
/// </summary>
/// <param name="Channel">按渠道筛选：Email、Sms</param>
/// <param name="Page">页码，从 1 开始</param>
/// <param name="PageSize">每页数量，1 到 200，默认 20</param>
/// <param name="Sort">排序，如 -capturedAt：字段名前加 - 表示倒序，默认新的在前</param>
public sealed record ListCapturedRequest(string? Channel, int Page = 1, int PageSize = 20, string? Sort = null);
