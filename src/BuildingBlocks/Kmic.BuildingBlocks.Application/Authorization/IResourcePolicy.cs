using Kmic.BuildingBlocks.Application.Access;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 授权第 3 层：这个人和这条数据的关系是否允许这个动作；每类资源一个
/// </summary>
/// <remarks>
/// 纯函数：不访问数据库、不调用外部服务，因此 allowedActions 与测试可以直接复用它；没有特殊规则的动作也要在 switch 中显式返回 Allow
/// </remarks>
public interface IResourcePolicy<in TResource, in TAction>
{
    /// <summary>
    /// 判定
    /// </summary>
    PolicyDecision Evaluate(IAccessContext access, TResource resource, TAction action);
}
