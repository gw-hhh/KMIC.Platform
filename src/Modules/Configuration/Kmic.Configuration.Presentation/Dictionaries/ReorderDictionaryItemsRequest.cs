namespace Kmic.Configuration.Presentation.Dictionaries;

/// <summary>
/// 调整字典项次序的请求
/// </summary>
/// <param name="ExpectedVersion">字典类型的预期版本号</param>
/// <param name="ParentCode">父项编码</param>
/// <param name="OrderedCodes">按新顺序排列的全部同级字典项编码（含停用项）</param>
public sealed record ReorderDictionaryItemsRequest(long ExpectedVersion, string? ParentCode, IReadOnlyList<string> OrderedCodes);
