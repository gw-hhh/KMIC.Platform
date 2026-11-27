namespace Kmic.Configuration.Presentation.Dictionaries;

/// <param name="ExpectedVersion">字典类型的预期版本号</param>
/// <param name="Name">新的显示名</param>
public sealed record RenameDictionaryItemRequest(long ExpectedVersion, string Name);
