using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens;

/// <summary>
/// 示例标本的资源策略：只有负责人可以提交、关闭和增删附件
/// </summary>
/// <remarks>
/// 策略只看资源和身份，不访问数据库；状态能不能流转由状态机判断，不在这里重复
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
