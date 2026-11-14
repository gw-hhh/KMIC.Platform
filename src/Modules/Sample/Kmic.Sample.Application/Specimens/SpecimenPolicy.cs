using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens;

/// <summary>
/// 示例的操作权限
/// </summary>
/// <remarks>
/// 提交、关闭和增删附件要求当前用户是负责人。
/// 这里只检查身份与归属，不访问数据库；状态限制由各操作检查
/// </remarks>
public sealed class SpecimenPolicy : IResourcePolicy<Specimen, SpecimenAction>
{
    /// <inheritdoc />
    public PolicyDecision Evaluate(IAccessContext access, Specimen resource, SpecimenAction action)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(action);

        return resource.OwnerId == access.UserId ? PolicyDecision.Allow : PolicyDecision.Deny(DenialReason.ResourceDenied, SampleErrors.NotOwner);
    }
}
