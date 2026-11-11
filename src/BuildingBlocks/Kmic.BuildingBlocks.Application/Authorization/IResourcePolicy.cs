using Kmic.BuildingBlocks.Application.Access;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 资源策略
/// </summary>
/// <remarks>
/// 不能访问数据库或外部服务，可用动作和测试都直接调用它。没有特殊规则的动作也要在 switch 里返回 Allow
/// </remarks>
public interface IResourcePolicy<in TResource, in TAction>
{
    /// <summary>
    /// 判断能否执行
    /// </summary>
    PolicyDecision Evaluate(IAccessContext access, TResource resource, TAction action);
}
