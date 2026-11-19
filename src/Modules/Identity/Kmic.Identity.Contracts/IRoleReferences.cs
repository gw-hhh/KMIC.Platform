using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.Identity.Contracts;

/// <summary>
/// 供其他模块批量解析角色引用
/// </summary>
/// <remarks>
/// 找不到的 ID 不返回，停用的也能查到
/// </remarks>
public interface IRoleReferences
{
    /// <summary>
    /// 批量解析角色引用
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
