namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 范围对象目录中的一项
/// </summary>
/// <param name="Code">编码，格式“模块.对象”</param>
/// <param name="Module">所属模块</param>
/// <param name="DisplayName">显示名</param>
public sealed record DataScopeObjectDefinition(string Code, string Module, string DisplayName);
