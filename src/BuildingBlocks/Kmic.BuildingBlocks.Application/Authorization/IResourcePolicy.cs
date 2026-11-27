using Kmic.BuildingBlocks.Application.Access;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>资源授权策略；判定须保持纯内存计算，禁止访问数据库或外部服务。</summary>
public interface IResourcePolicy<in TResource, in TAction>
{
    PolicyDecision Evaluate(IAccessContext access, TResource resource, TAction action);
}
