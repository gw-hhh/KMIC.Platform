namespace Kmic.Configuration.Contracts;

/// <summary>
/// 字典项
/// </summary>
/// <param name="Code">字典项编码</param>
/// <param name="Name">显示名</param>
/// <param name="ParentCode">层级字典里父项的编码，顶层为空</param>
/// <param name="SortOrder">在同一父项下的次序</param>
/// <param name="IsActive">是否启用</param>
public sealed record DictionaryEntry(string Code, string Name, string? ParentCode, int SortOrder, bool IsActive);
