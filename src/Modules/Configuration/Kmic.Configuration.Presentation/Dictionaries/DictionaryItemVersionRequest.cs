namespace Kmic.Configuration.Presentation.Dictionaries;

/// <summary>
/// 停用、启用字典项的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的类型版本号，别人改过就返回 409 Common.VersionConflict</param>
public sealed record DictionaryItemVersionRequest(long ExpectedVersion);
