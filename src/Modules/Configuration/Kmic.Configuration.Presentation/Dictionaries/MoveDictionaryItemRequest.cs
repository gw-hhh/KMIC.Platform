namespace Kmic.Configuration.Presentation.Dictionaries;

/// <summary>
/// 移动字典项的请求
/// </summary>
/// <param name="ExpectedVersion">字典类型的预期版本号，不匹配时返回 409 Common.VersionConflict</param>
/// <param name="NewParentCode">新父项编码；为 null 或空白时移至顶层，不能指定自身或后代</param>
public sealed record MoveDictionaryItemRequest(long ExpectedVersion, string? NewParentCode);
