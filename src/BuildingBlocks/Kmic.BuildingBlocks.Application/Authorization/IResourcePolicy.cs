using Kmic.BuildingBlocks.Application.Access;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 资源策略
/// </summary>
/// <remarks>
/// 不能访问数据库或外部服务
/// </remarks>
public interface IResourcePolicy<in TResource, in TAction>
{
    /// <summary>
    /// 判断能否执行
    /// </summary>
    PolicyDecision Evaluate(IAccessContext access, TResource resource, TAction action);
}
