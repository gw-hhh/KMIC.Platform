using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Identity.Application.Users.List;

public static class UserSort
{
    /// <summary>用户列表排序字段表，默认按姓名排序。</summary>
    public static readonly SortMap<UserListItem> Map = SortMap<UserListItem>
        .Field("displayName", x => x.DisplayName)
        .Field("account", x => x.Account)
        .Field("employeeNo", x => x.EmployeeNo)
        .Default("displayName");
}
