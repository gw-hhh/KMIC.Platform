using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Configuration.Application.Settings;

public sealed class SettingActions : ResourceActionCatalog<SettingTarget, SettingAction>
{
    protected override void Define(IResourceActionBuilder<SettingTarget, SettingAction> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Action(SettingAction.Edit, ConfigurationPermissions.SettingView);
        builder.Action(SettingAction.Reset, ConfigurationPermissions.SettingView);
    }
}
