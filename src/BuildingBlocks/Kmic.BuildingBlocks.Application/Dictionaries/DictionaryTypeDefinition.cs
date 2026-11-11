namespace Kmic.BuildingBlocks.Application.Dictionaries;

/// <summary>
/// 字典类型目录中的一项
/// </summary>
/// <param name="Code">编码，格式“模块.名称”</param>
/// <param name="Module">所属模块</param>
/// <param name="DisplayName">显示名</param>
/// <param name="IsHierarchical">字典项是否分层级</param>
public sealed record DictionaryTypeDefinition(string Code, string Module, string DisplayName, bool IsHierarchical);
