using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Errors;

namespace Kmic.Files.Application.Uploads;

/// <summary>
/// 上传档案的资源策略：操作者当前的角色有档案声明的权限点才能上传
/// </summary>
/// <remarks>
/// 上传本身就要求有编辑这类业务对象的能力，避免任何登录用户都能往存储里塞文件。纯函数，不查库
/// </remarks>
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
