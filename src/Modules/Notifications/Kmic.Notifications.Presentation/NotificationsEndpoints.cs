using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Notifications.Contracts;

namespace Kmic.Notifications.Presentation;

public sealed class NotificationsEndpoints : ModuleEndpoints
{
    public override string Module => NotificationsModuleInfo.Name;
}
