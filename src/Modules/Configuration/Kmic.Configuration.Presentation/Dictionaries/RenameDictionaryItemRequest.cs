namespace Kmic.Configuration.Presentation.Dictionaries;

/// <summary>
/// 修改字典项显示名的请求
/// </summary>
/// <param name="ExpectedVersion">字典类型的预期版本号</param>
/// <param name="Name">新的显示名</param>
public sealed record RenameDictionaryItemRequest(long ExpectedVersion, string Name);
