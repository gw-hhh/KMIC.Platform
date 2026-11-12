using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Errors;

namespace Kmic.Configuration.Application.Settings;

/// <summary>
/// 系统设置的资源策略：改一个设置要有它声明的修改权限点
/// </summary>
/// <remarks>
/// 命令上静态声明的只能是一个权限点，而每个设置的修改权限点不同，所以命令声明“查看设置”这个基础权限点，
/// 再由这条策略检查该设置自己的修改权限点。这样列表也能按每个设置给出“当前用户能不能改”
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
