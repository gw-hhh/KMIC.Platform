using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Enumerations;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>资源动作目录，只返回当前用户允许执行的动作。</summary>
public abstract class ResourceActionCatalog<TResource, TAction> : IResourceActionCatalog
    where TAction : notnull
{
    private readonly Lazy<IReadOnlyList<ResourceAction>> _actions;

    protected ResourceActionCatalog() => _actions = new(Collect);

    public IEnumerable<string> Permissions => _actions.Value.Select(a => a.Permission);

    /// <summary>评估当前用户对指定资源可执行的动作编码，按声明顺序返回。</summary>
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
