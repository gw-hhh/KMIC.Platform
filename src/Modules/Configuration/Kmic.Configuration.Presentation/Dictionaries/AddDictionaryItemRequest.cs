namespace Kmic.Configuration.Presentation.Dictionaries;

/// <summary>
/// 添加字典项的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的类型版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="Code">编码，字母、数字、点、短横线、下划线，1 到 50 位，创建后不能改</param>
/// <param name="Name">显示名，最多 200 个字符</param>
/// <param name="ParentCode">父项编码；只有层级字典可以给</param>
/// <param name="SortOrder">次序；不给就排在同级最后</param>
public sealed record AddDictionaryItemRequest(long ExpectedVersion, string Code, string Name, string? ParentCode, int? SortOrder);
