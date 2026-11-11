using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 资源授权
/// </summary>
public interface IResourceAuthorizer<in TResource, in TAction>
{
    /// <summary>
    /// 检查当前用户能否对这条数据执行该动作
    /// </summary>
    Result Check(TResource resource, TAction action);
}
