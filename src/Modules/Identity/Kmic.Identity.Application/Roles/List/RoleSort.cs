using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Identity.Application.Roles.List;

/// <summary>
/// 角色列表允许排序的字段
/// </summary>
public static class RoleSort
{
    /// <summary>
    /// 排序字段表，默认按次序再按名称
    /// </summary>
    public static readonly SortMap<RoleListItem> Map = SortMap<RoleListItem>
        .Field("name", x => x.Name)
        .Field("sortOrder", x => x.SortOrder)
        .Field("holderCount", x => x.HolderCount)
        .Default("sortOrder");
}
