namespace Kmic.Configuration.Presentation.Settings;

/// <summary>
/// 查询设置变更历史的参数
/// </summary>
/// <param name="Key">设置键筛选，未指定时查询全部设置</param>
/// <param name="From">起始时间，含</param>
/// <param name="To">结束时间，不含</param>
/// <param name="Page">页码，从 1 开始</param>
/// <param name="PageSize">每页数量，1 到 200，默认 20</param>
/// <param name="Sort">排序，如 -changedAt,key：逗号分隔，字段名前加 - 表示倒序，最多 3 个字段</param>
public sealed record ListSettingChangesRequest(string? Key, DateTimeOffset? From, DateTimeOffset? To, int Page = 1, int PageSize = 20, string? Sort = null);
