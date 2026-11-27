using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.BuildingBlocks.Application.Auditing;

/// <summary>业务历史操作人与角色引用解析器，由 Identity 模块实现。</summary>
public interface IActorReferences
{
    /// <summary>按用户 ID 批量解析引用信息，未匹配项不返回。</summary>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveUsersAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct);

    /// <summary>按角色 ID 批量解析引用信息，未匹配项不返回。</summary>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveRolesAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken ct);
}
