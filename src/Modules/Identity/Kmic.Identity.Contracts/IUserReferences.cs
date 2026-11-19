using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.Identity.Contracts;

/// <summary>
/// 供其他模块批量解析用户引用
/// </summary>
/// <remarks>
/// 找不到的 ID 不返回，禁用的也能查到
/// </remarks>
public interface IUserReferences
{
    /// <summary>
    /// 批量解析用户引用
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
