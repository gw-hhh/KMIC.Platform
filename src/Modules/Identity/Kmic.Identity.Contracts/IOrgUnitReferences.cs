using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.Identity.Contracts;

/// <summary>
/// 供其他模块批量解析组织单元引用
/// </summary>
/// <remarks>
/// 找不到的 ID 不返回，由调用方显示占位文字；停用的组织单元也能解析并带上状态。允许重复 ID，批次大小不限，实现自动分批
/// </remarks>
public interface IOrgUnitReferences
{
    /// <summary>
    /// 批量解析组织单元引用
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
