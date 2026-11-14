using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.Identity.Contracts;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// <see cref="IActorReferences"/> 的实现：业务历史里的人员和角色名
/// </summary>
public sealed class ActorReferences(IUserReferences users, IRoleReferences roles) : IActorReferences
{
    /// <inheritdoc />
    public Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveUsersAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct) =>
        users.ResolveAsync(userIds, ct);

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveRolesAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken ct) =>
        roles.ResolveAsync(roleIds, ct);
}
