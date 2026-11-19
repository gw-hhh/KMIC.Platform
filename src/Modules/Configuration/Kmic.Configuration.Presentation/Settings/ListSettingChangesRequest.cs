namespace Kmic.Configuration.Presentation.Settings;

/// <summary>
/// 查询设置变更历史的参数
/// </summary>
/// <param name="Key">设置键筛选</param>
/// <param name="From">起始时间</param>
/// <param name="To">结束时间</param>
/// <param name="Page">页码</param>
/// <param name="PageSize">每页数量</param>
/// <param name="Sort">排序</param>
public sealed record ListSettingChangesRequest(string? Key, DateTimeOffset? From, DateTimeOffset? To, int Page = 1, int PageSize = 20, string? Sort = null);
