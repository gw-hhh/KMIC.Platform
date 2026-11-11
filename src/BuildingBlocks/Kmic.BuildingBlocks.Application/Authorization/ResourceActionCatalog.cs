using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Enumerations;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 资源动作目录
/// </summary>
/// <remarks>
/// 有权限点、资源策略允许、状态检查通过，三项都满足的动作才会返回
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
    /// 全部动作用到的权限点
    /// </summary>
    public IEnumerable<string> Permissions => _actions.Value.Select(a => a.Permission);

    /// <summary>
    /// 当前用户可用的动作编码，按声明顺序
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
    /// 声明动作
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
