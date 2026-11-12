using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Configuration.Application.Settings;

/// <summary>
/// 系统设置的资源动作目录：列表里每个设置的“可用动作”据此计算
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
