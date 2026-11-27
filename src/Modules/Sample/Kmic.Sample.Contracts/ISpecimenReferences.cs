using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.Sample.Contracts;

/// <summary>供其他模块查询示例引用信息；不存在或已删除的示例不予返回</summary>
public interface ISpecimenReferences
{
    /// <summary>批量解析示例引用</summary>
    /// <exception cref="InvalidOperationException">查询失败</exception>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
