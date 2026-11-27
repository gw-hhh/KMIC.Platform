using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Identity.Application.Roles.List;

public static class RoleSort
{
    /// <summary>角色列表排序字段表，默认按角色次序排序。</summary>
    public static readonly SortMap<RoleListItem> Map = SortMap<RoleListItem>
        .Field("name", x => x.Name)
        .Field("sortOrder", x => x.SortOrder)
        .Field("holderCount", x => x.HolderCount)
        .Default("sortOrder");
}
