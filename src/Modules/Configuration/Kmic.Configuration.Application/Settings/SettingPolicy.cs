using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Errors;

namespace Kmic.Configuration.Application.Settings;

/// <summary>按设置声明的权限点授权修改与恢复默认</summary>
public sealed class SettingPolicy : IResourcePolicy<SettingTarget, SettingAction>
{
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
