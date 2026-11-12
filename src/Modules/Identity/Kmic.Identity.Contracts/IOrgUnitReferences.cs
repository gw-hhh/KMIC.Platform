using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.Identity.Contracts;

/// <summary>
/// 其他模块解析组织单元引用时用的接口
/// </summary>
/// <remarks>
/// 只返回显示所需的最少信息：ID、名称、编码、状态。按 ID 批量查询，没有找到的 ID 不会出现在结果里，
/// 调用方显示为“已删除”之类的占位。参数里 ID 可以重复，数量没有上限，实现会自动分批。
/// 组织单元不删除只停用，所以已停用的组织单元仍能解析出来，状态为已停用
/// </remarks>
public interface IOrgUnitReferences
{
    /// <summary>
    /// 批量解析组织单元引用
    /// </summary>
    /// <param name="ids">组织单元 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
