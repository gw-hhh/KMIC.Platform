namespace Kmic.BuildingBlocks.Contracts.References;

/// <summary>引用信息（跨模块引用查询不按数据权限过滤，仅返回展示用基础字段）。</summary>
/// <param name="Id">被引用对象的 ID</param>
/// <param name="DisplayName">显示名称</param>
/// <param name="Code">编码</param>
/// <param name="Status">当前状态</param>
public sealed record ReferenceInfo(Guid Id, string DisplayName, string? Code, ReferenceStatus Status);
