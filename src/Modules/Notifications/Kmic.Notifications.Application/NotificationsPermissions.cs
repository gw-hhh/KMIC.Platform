using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Notifications.Application;

/// <summary>
/// Notifications 模块的权限点
/// </summary>
/// <remarks>
/// 每个权限点是一个常量，写成“Notifications.资源.动作”，并标 [Permission("显示名")]
/// </remarks>
[PermissionGroup("Notifications", "通知")]
public static class NotificationsPermissions
{
}
