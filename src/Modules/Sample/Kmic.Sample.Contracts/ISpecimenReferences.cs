using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.Sample.Contracts;

/// <summary>
/// 其他模块解析示例引用时用的接口
/// </summary>
/// <remarks>
/// 只返回 ID、编号、名称和状态。
/// 不存在或已删除的示例不出现在结果中，调用方应显示“已删除”等占位文字。
/// 参数可包含重复 ID，不限制传入数量；实现会自动去重并分批查询
/// </remarks>
public interface ISpecimenReferences
{
    /// <summary>
    /// 批量解析示例引用
    /// </summary>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
