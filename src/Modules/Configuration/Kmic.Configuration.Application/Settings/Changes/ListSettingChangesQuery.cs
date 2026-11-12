using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Settings.Changes;

/// <summary>
/// 查询系统设置的变更历史：谁在什么时候把什么改成了什么，为什么
/// </summary>
/// <param name="Key">只看这个设置的；不给就是全部</param>
/// <param name="From">起始时间，含</param>
/// <param name="To">结束时间，不含</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[RequirePermission(ConfigurationPermissions.SettingView)]
public sealed record ListSettingChangesQuery(string? Key, DateTimeOffset? From, DateTimeOffset? To, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<SettingChangeItem>>;
