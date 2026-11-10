using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 处理器使用的第 3 层检查：取访问上下文、系统身份放行、拒绝时写安全事件
/// </summary>
public interface IResourceAuthorizer<in TResource, in TAction>
{
    /// <summary>
    /// 检查当前身份能否对资源执行动作
    /// </summary>
    Result Check(TResource resource, TAction action);
}
