namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 数据范围对象定义
/// </summary>
/// <param name="Code">编码，如 Sales.Quotation</param>
/// <param name="Module">所属模块</param>
/// <param name="DisplayName">显示名称</param>
public sealed record DataScopeObjectDefinition(string Code, string Module, string DisplayName);
