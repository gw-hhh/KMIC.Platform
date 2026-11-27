using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.Identity.Contracts;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>为业务历史解析用户和角色名称</summary>
public sealed class ActorReferences(IUserReferences users, IRoleReferences roles) : IActorReferences
{
    public Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveUsersAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct) =>
        users.ResolveAsync(userIds, ct);

    public Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveRolesAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken ct) =>
        roles.ResolveAsync(roleIds, ct);
}
