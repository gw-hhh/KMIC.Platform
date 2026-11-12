using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Customer.Application;

/// <summary>
/// Customer 模块的权限点
/// </summary>
/// <remarks>
/// 每个权限点是一个常量，写成“Customer.资源.动作”，并标 [Permission("显示名")]
/// </remarks>
[PermissionGroup("Customer", "客户")]
public static class CustomerPermissions
{
}
