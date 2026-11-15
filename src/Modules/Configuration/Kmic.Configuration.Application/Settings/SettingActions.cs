using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Configuration.Application.Settings;

/// <summary>
/// 系统设置的操作目录，用于计算各设置的可用操作
/// </summary>
public sealed class SettingActions : ResourceActionCatalog<SettingTarget, SettingAction>
{
    /// <inheritdoc />
    protected override void Define(IResourceActionBuilder<SettingTarget, SettingAction> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Action(SettingAction.Edit, ConfigurationPermissions.SettingView);
        builder.Action(SettingAction.Reset, ConfigurationPermissions.SettingView);
    }
}
