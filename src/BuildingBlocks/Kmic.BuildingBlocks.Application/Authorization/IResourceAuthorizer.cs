using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

public interface IResourceAuthorizer<in TResource, in TAction>
{
    /// <summary>检查当前用户能否对指定资源执行该动作。</summary>
    Result Check(TResource resource, TAction action);
}
