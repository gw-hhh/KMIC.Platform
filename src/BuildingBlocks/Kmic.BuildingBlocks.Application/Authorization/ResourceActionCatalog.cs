using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Enumerations;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 每类资源一份动作目录，allowedActions 由它计算，与真实检查同源
/// </summary>
/// <remarks>
/// 一个动作出现在结果中，当且仅当：权限点在当前角色中 ∧ 资源策略允许 ∧ 状态检查通过；第 2 层由加载本身保证
/// </remarks>
public abstract class ResourceActionCatalog<TResource, TAction> : IResourceActionCatalog
    where TAction : notnull
{
    private readonly Lazy<IReadOnlyList<ResourceAction>> _actions;

    /// <summary>
    /// 由派生类调用
    /// </summary>
    protected ResourceActionCatalog() => _actions = new(Collect);

    /// <summary>
    /// 全部动作的权限点，供权限点目录核对“无死权限”
    /// </summary>
    public IEnumerable<string> Permissions => _actions.Value.Select(a => a.Permission);

    /// <summary>
    /// 当前访问上下文可用的动作编码，按声明顺序
    /// </summary>
    public IReadOnlyList<string> Evaluate(IAccessContext access, IResourcePolicy<TResource, TAction> policy, TResource resource)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(policy);

        var readOnlySession = access.ImpersonatorUserId is not null && !access.ImpersonationAllowsWrite;
        return
        [
            .. _actions.Value
                .Where(a => !readOnlySession || a.ReadOnly)
                .Where(a => access.Permissions.Contains(a.Permission))
                .Where(a => policy.Evaluate(access, resource, a.Action).Allowed)
                .Where(a => a.State is null || a.State(resource).IsSuccess)
                .Select(a => CodeOf(a.Action)),
        ];
    }

    /// <summary>
    /// 声明全部动作
    /// </summary>
    protected abstract void Define(IResourceActionBuilder<TResource, TAction> builder);

    private static string CodeOf(TAction action) =>
        action is IEnumeration enumeration ? enumeration.Code : action.ToString() ?? string.Empty;

    private List<ResourceAction> Collect()
    {
        var builder = new Builder();
        Define(builder);
        return builder.Actions;
    }

    private sealed record ResourceAction(TAction Action, string Permission, Func<TResource, Result>? State, bool ReadOnly);

    private sealed class Builder : IResourceActionBuilder<TResource, TAction>
    {
        public List<ResourceAction> Actions { get; } = [];

        public void Action(TAction action, string permission, Func<TResource, Result>? state = null, bool readOnly = false)
        {
            ArgumentNullException.ThrowIfNull(action);
            ArgumentException.ThrowIfNullOrWhiteSpace(permission);
            Actions.Add(new ResourceAction(action, permission, state, readOnly));
        }
    }
}
