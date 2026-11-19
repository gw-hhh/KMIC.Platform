using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.Identity.Contracts;

/// <summary>
/// 供其他模块批量解析用户引用
/// </summary>
/// <remarks>
/// 找不到的 ID 不返回，由调用方显示占位文字；禁用的用户也能解析并带上状态。允许重复 ID，批次大小不限，实现自动分批；查询失败时抛异常
/// </remarks>
public interface IUserReferences
{
    /// <summary>
    /// 批量解析用户引用
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
