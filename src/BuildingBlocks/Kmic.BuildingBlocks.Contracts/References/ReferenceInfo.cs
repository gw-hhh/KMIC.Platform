namespace Kmic.BuildingBlocks.Contracts.References;

/// <summary>
/// 引用信息
/// </summary>
/// <remarks>
/// 这类查询不按数据权限过滤，所以只返回显示用的几个字段
/// </remarks>
/// <param name="Id">被引用对象的 ID</param>
/// <param name="DisplayName">显示名称</param>
/// <param name="Code">编码</param>
/// <param name="Status">当前状态</param>
public sealed record ReferenceInfo(Guid Id, string DisplayName, string? Code, ReferenceStatus Status);
