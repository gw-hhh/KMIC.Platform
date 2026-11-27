namespace Kmic.Configuration.Contracts;

/// <summary>字典项</summary>
/// <param name="ParentCode">层级字典的父项编码，顶层项为空</param>
/// <param name="SortOrder">同级排序值</param>
public sealed record DictionaryEntry(string Code, string Name, string? ParentCode, int SortOrder, bool IsActive);
