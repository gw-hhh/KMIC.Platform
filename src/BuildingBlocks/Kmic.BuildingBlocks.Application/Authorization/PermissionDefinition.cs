namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 权限点定义
/// </summary>
/// <param name="Code">编码，如 Sales.Quotation.Approve</param>
/// <param name="Module">所属模块</param>
/// <param name="Group">分组名称</param>
/// <param name="DisplayName">显示名称</param>
/// <param name="Sensitive">是否敏感</param>
/// <param name="RenamedFrom">改名前的编码，没有改过名为空</param>
public sealed record PermissionDefinition(string Code, string Module, string Group, string DisplayName, bool Sensitive, string? RenamedFrom = null);
