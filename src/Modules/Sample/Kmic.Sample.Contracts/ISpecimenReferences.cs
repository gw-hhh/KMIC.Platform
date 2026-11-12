using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.Sample.Contracts;

/// <summary>
/// 其他模块解析示例标本引用时用的接口
/// </summary>
/// <remarks>
/// 只返回显示所需的最少信息：ID、编号、名称、状态。按 ID 批量查询，没有找到（不存在或已删除）的 ID 不会出现在结果里，
/// 调用方显示为“已删除”之类的占位。参数里 ID 可以重复，数量没有上限，实现会自动分批
/// </remarks>
public interface ISpecimenReferences
{
    /// <summary>
    /// 批量解析标本引用
    /// </summary>
    /// <param name="ids">标本 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
