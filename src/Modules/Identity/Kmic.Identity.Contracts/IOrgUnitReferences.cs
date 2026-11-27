using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.Identity.Contracts;

/// <summary>跨模块批量解析组织单元引用，仅返回存在的记录（含已停用项）。</summary>
public interface IOrgUnitReferences
{
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
