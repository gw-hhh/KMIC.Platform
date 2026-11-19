using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Errors;

namespace Kmic.Files.Application.Uploads;

/// <summary>
/// 按上传配置检查当前角色的上传权限
/// </summary>
public sealed class UploadProfilePolicy : IResourcePolicy<UploadProfileTarget, UploadAction>
{
    /// <inheritdoc />
    public PolicyDecision Evaluate(IAccessContext access, UploadProfileTarget resource, UploadAction action)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(action);

        return access.Permissions.Contains(resource.Permission)
            ? PolicyDecision.Allow
            : PolicyDecision.Deny(DenialReason.MissingPermission, CommonErrors.MissingPermission);
    }
}
