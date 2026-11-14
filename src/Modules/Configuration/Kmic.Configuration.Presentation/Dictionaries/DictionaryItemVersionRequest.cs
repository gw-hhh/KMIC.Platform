namespace Kmic.Configuration.Presentation.Dictionaries;

/// <summary>
/// 停用、启用字典项的请求
/// </summary>
/// <param name="ExpectedVersion">字典类型的预期版本号，不匹配时返回 409 Common.VersionConflict</param>
public sealed record DictionaryItemVersionRequest(long ExpectedVersion);
