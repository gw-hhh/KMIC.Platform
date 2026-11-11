namespace Kmic.BuildingBlocks.Application.Dictionaries;

/// <summary>
/// 字典类型定义
/// </summary>
/// <param name="Code">编码，如 Sales.Industry</param>
/// <param name="Module">所属模块</param>
/// <param name="DisplayName">显示名称</param>
/// <param name="IsHierarchical">是否分层</param>
public sealed record DictionaryTypeDefinition(string Code, string Module, string DisplayName, bool IsHierarchical);
