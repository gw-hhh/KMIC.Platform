using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Errors;

namespace Kmic.Configuration.Application.Settings;

/// <summary>
/// 按设置声明的权限点授权修改及恢复默认操作
/// </summary>
/// <remarks>
/// 命令要求“查看设置”基础权限，本策略再检查目标设置声明的修改权限。
/// 设置列表同样使用此策略计算当前用户的可用动作
/// </remarks>
public sealed class SettingPolicy : IResourcePolicy<SettingTarget, SettingAction>
{
    /// <inheritdoc />
    public PolicyDecision Evaluate(IAccessContext access, SettingTarget resource, SettingAction action)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(action);

        return access.Permissions.Contains(resource.EditPermission)
            ? PolicyDecision.Allow
            : PolicyDecision.Deny(DenialReason.MissingPermission, CommonErrors.MissingPermission);
    }
}
