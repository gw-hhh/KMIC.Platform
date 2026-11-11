using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

internal sealed class ResourceAuthorizer<TResource, TAction>(
    IResourcePolicy<TResource, TAction> policy,
    IExecutionContext context,
    ISecurityEventWriter securityEvents) : IResourceAuthorizer<TResource, TAction>
{
    public Result Check(TResource resource, TAction action)
    {
        // 系统身份直接放行：能走到这里，说明用例已声明允许系统调用
        if (context.User.IsSystem)
        {
            return Result.Success();
        }

        var access = context.Access
            ?? throw new InvalidOperationException("用户身份没有访问上下文。");
        var decision = policy.Evaluate(access, resource, action);
        if (decision.Allowed)
        {
            return Result.Success();
        }

        securityEvents.Write(new SecurityEvent("AuthorizationDenied", SecuritySeverity.Warning, decision.Reason?.ToString())
        {
            ResourceType = typeof(TResource).Name,
        });
        return decision.Error ?? throw new InvalidOperationException($"{policy.GetType().Name} 拒绝时没有给出错误。");
    }
}
