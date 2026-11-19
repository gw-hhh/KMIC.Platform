using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.BuildingBlocks.Application.Auditing;

/// <summary>
/// 解析业务历史里的操作人和角色，由 Identity 实现
/// </summary>
public interface IActorReferences
{
    /// <summary>
    /// 按用户 ID 批量取引用信息，查不到的不在结果里
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveUsersAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct);

    /// <summary>
    /// 按角色 ID 批量取引用信息，查不到的不在结果里
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveRolesAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken ct);
}
