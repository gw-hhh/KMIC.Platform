namespace Kmic.Notifications.Presentation.Deliveries;

/// <summary>
/// 查询投递记录的参数
/// </summary>
/// <param name="RecipientUserId">按收件用户筛选</param>
/// <param name="Channel">按渠道筛选：Email、Sms</param>
/// <param name="Status">按状态筛选：Pending、Sending、Sent、Failed、Skipped、Captured</param>
/// <param name="TemplateKey">按模板键筛选</param>
/// <param name="From">创建时间起，含</param>
/// <param name="To">创建时间止，不含</param>
/// <param name="Page">页码，从 1 开始</param>
/// <param name="PageSize">每页数量，1 到 200，默认 20</param>
/// <param name="Sort">排序，如 -createdAt：逗号分隔，字段名前加 - 表示倒序，默认新的在前</param>
public sealed record ListDeliveriesRequest(
    Guid? RecipientUserId,
    string? Channel,
    string? Status,
    string? TemplateKey,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page = 1,
    int PageSize = 20,
    string? Sort = null);
