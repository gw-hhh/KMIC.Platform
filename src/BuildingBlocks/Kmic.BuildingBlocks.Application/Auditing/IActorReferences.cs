using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.BuildingBlocks.Application.Auditing;

/// <summary>
/// 解析业务历史里的操作人和角色
/// </summary>
/// <remarks>
/// 由 Identity 实现。框架不能引用模块的契约，所以用这个接口取显示名
/// </remarks>
public interface IActorReferences
{
    /// <summary>
    /// 按用户 ID 批量取引用信息，查不到的不在结果里
    /// </summary>
    /// <param name="userIds">用户 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveUsersAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct);

    /// <summary>
    /// 按角色 ID 批量取引用信息，查不到的不在结果里
    /// </summary>
    /// <param name="roleIds">角色 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveRolesAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken ct);
}
