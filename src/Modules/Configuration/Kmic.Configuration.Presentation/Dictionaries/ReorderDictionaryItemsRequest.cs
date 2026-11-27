namespace Kmic.Configuration.Presentation.Dictionaries;

/// <param name="ExpectedVersion">字典类型的预期版本号</param>
/// <param name="ParentCode">父项编码</param>
/// <param name="OrderedCodes">字典项编码列表，按新顺序排列全部同级项（含停用项）</param>
public sealed record ReorderDictionaryItemsRequest(long ExpectedVersion, string? ParentCode, IReadOnlyList<string> OrderedCodes);
