using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 资源动作声明
/// </summary>
public interface IResourceActionBuilder<TResource, in TAction>
{
    /// <summary>
    /// 声明动作
    /// </summary>
    /// <param name="state">状态检查，如状态机的 CanFire；没有状态限制时为空</param>
    void Action(TAction action, string permission, Func<TResource, Result>? state = null, bool readOnly = false);
}
