namespace Kmic.Configuration.Contracts;

/// <summary>
/// 字典里的一项
/// </summary>
/// <param name="Code">编码，业务数据里存的就是它</param>
/// <param name="Name">显示名</param>
/// <param name="ParentCode">层级字典里父项的编码；顶层为空</param>
/// <param name="SortOrder">在同一父项下的次序</param>
/// <param name="IsActive">是否启用；停用的项历史数据仍然显示，新数据不能再选</param>
public sealed record DictionaryEntry(string Code, string Name, string? ParentCode, int SortOrder, bool IsActive);
