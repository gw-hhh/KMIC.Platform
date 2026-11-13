using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Notifications.Contracts;

namespace Kmic.Notifications.Presentation;

/// <summary>
/// Notifications 模块的端点注册类
/// </summary>
public sealed class NotificationsEndpoints : ModuleEndpoints
{
    /// <inheritdoc />
    public override string Module => NotificationsModuleInfo.Name;
}
