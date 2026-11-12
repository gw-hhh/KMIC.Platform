namespace Kmic.Configuration.Presentation.Dictionaries;

/// <summary>
/// 调整字典项次序的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的类型版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="ParentCode">父项编码；顶层不给</param>
/// <param name="OrderedCodes">按新次序排好的该父项下的全部字典项编码，不能多也不能少，不能重复</param>
public sealed record ReorderDictionaryItemsRequest(long ExpectedVersion, string? ParentCode, IReadOnlyList<string> OrderedCodes);
