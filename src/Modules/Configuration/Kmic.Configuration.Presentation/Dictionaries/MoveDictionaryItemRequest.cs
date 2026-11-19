namespace Kmic.Configuration.Presentation.Dictionaries;

/// <summary>
/// 移动字典项的请求
/// </summary>
/// <param name="ExpectedVersion">字典类型的预期版本号</param>
/// <param name="NewParentCode">新父项编码</param>
public sealed record MoveDictionaryItemRequest(long ExpectedVersion, string? NewParentCode);
