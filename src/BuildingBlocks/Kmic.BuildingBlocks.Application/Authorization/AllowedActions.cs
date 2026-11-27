using Kmic.BuildingBlocks.Application.Execution;

namespace Kmic.BuildingBlocks.Application.Authorization;

internal sealed class AllowedActions<TResource, TAction>(
    ResourceActionCatalog<TResource, TAction> catalog,
    IResourcePolicy<TResource, TAction> policy,
    IExecutionContext context) : IAllowedActions<TResource>
    where TAction : notnull
{
    // 系统身份与匿名请求无需返回界面操作动作
    public IReadOnlyList<string> For(TResource resource) =>
        context.Access is { } access ? catalog.Evaluate(access, policy, resource) : [];
}
