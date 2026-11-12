namespace Kmic.Configuration.Presentation.Dictionaries;

/// <summary>
/// 修改字典项显示名的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的类型版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="Name">新的显示名，最多 200 个字符</param>
public sealed record RenameDictionaryItemRequest(long ExpectedVersion, string Name);
