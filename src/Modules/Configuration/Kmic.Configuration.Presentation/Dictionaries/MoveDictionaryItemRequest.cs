namespace Kmic.Configuration.Presentation.Dictionaries;

/// <summary>
/// 移动字典项的请求
/// </summary>
/// <param name="ExpectedVersion">字典类型的预期版本号，不匹配时返回 409 Common.VersionConflict</param>
/// <param name="NewParentCode">新的父项编码；不给就移到顶层。不能是自己或自己的后代</param>
public sealed record MoveDictionaryItemRequest(long ExpectedVersion, string? NewParentCode);
