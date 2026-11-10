namespace Kmic.BuildingBlocks.Contracts.References;

/// <summary>
/// 引用解析的结果：只含显示所需的最少字段
/// </summary>
/// <remarks>
/// Reference 类 Contracts 查询不套数据范围，所以返回类型限定为它，防止被扩展成读取完整数据的通道（05-07 D3）
/// </remarks>
/// <param name="Id">被引用对象的 ID</param>
/// <param name="DisplayName">显示名称</param>
/// <param name="Code">业务编码，没有编码的对象为空</param>
/// <param name="Status">当前状态</param>
public sealed record ReferenceInfo(Guid Id, string DisplayName, string? Code, ReferenceStatus Status);
