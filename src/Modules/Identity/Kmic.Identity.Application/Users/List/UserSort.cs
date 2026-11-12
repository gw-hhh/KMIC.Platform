using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Identity.Application.Users.List;

/// <summary>
/// 用户列表允许排序的字段
/// </summary>
public static class UserSort
{
    /// <summary>
    /// 排序字段表，默认按姓名
    /// </summary>
    public static readonly SortMap<UserListItem> Map = SortMap<UserListItem>
        .Field("displayName", x => x.DisplayName)
        .Field("account", x => x.Account)
        .Field("employeeNo", x => x.EmployeeNo)
        .Default("displayName");
}
