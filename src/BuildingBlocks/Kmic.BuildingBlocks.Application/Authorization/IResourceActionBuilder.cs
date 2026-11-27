using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

public interface IResourceActionBuilder<TResource, in TAction>
{
    /// <summary>声明资源动作；<paramref name="state"/> 为状态前置校验（如状态机 CanFire），无状态限制时传 null。</summary>
    void Action(TAction action, string permission, Func<TResource, Result>? state = null, bool readOnly = false);
}
