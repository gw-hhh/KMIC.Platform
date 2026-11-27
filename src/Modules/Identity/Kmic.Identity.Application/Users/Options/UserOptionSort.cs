using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Identity.Application.Users.Options;

// 选人下拉框固定按姓名排序，不对外开放排序字段
internal static class UserOptionSort
{
    public static readonly SortMap<UserOption> Map = SortMap<UserOption>
        .Field("displayName", x => x.DisplayName)
        .Default("displayName");
}
