namespace Kmic.Configuration.Presentation.Dictionaries;

/// <summary>
/// 添加字典项的请求
/// </summary>
/// <param name="ExpectedVersion">字典类型的预期版本号，不匹配时返回 409 Common.VersionConflict</param>
/// <param name="Code">编码，字母、数字、点、短横线、下划线，1 到 50 位，创建后不能改</param>
/// <param name="Name">显示名，最多 200 个字符</param>
/// <param name="ParentCode">父项编码；只有层级字典可以给</param>
/// <param name="SortOrder">排序值；未指定时排在同级末尾</param>
public sealed record AddDictionaryItemRequest(long ExpectedVersion, string Code, string Name, string? ParentCode, int? SortOrder);
