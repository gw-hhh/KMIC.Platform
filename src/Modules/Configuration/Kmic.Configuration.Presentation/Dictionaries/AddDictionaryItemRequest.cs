namespace Kmic.Configuration.Presentation.Dictionaries;

/// <param name="ExpectedVersion">字典类型的预期版本号</param>
/// <param name="Code">编码</param>
/// <param name="Name">显示名</param>
/// <param name="ParentCode">父项编码</param>
/// <param name="SortOrder">排序值</param>
public sealed record AddDictionaryItemRequest(long ExpectedVersion, string Code, string Name, string? ParentCode, int? SortOrder);
