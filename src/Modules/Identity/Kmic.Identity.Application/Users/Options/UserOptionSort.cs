using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Identity.Application.Users.Options;

/// <summary>
/// 选人下拉的排序：固定按姓名，不开放给调用方选择
/// </summary>
internal static class UserOptionSort
{
    public static readonly SortMap<UserOption> Map = SortMap<UserOption>
        .Field("displayName", x => x.DisplayName)
        .Default("displayName");
}
