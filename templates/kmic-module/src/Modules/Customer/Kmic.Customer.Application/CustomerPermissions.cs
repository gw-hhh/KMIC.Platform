using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Customer.Application;

/// <summary>在此声明权限常量（格式为“Customer.资源.动作”），并标注 [Permission("显示名")]</summary>
[PermissionGroup("Customer", "客户")]
public static class CustomerPermissions
{
}
