using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Settings.Changes;

/// <summary>
/// 查询系统设置变更历史，包含操作人、时间、新旧值和原因
/// </summary>
/// <param name="Key">设置键筛选</param>
/// <param name="From">起始时间</param>
/// <param name="To">结束时间</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(ConfigurationPermissions.SettingView)]
public sealed record ListSettingChangesQuery(string? Key, DateTimeOffset? From, DateTimeOffset? To, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<SettingChangeItem>>;
