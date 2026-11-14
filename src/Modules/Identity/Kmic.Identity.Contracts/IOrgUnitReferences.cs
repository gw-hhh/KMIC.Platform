using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.Identity.Contracts;

/// <summary>
/// 供其他模块批量解析组织单元引用
/// </summary>
/// <remarks>
/// 按 ID 返回显示信息及状态，未找到的 ID 不返回，由调用方显示占位文字。
/// 允许重复 ID，不限制调用方批次大小，实现自动分批；已停用组织单元仍可解析，并保留其状态
/// </remarks>
public interface IOrgUnitReferences
{
    /// <summary>
    /// 批量解析组织单元引用
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
