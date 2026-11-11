namespace Kmic.BuildingBlocks.Application.Dictionaries;

/// <summary>
/// 字典类型
/// </summary>
/// <param name="displayName">显示名称，如“行业”</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class DictionaryTypeAttribute(string displayName) : Attribute
{
    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; } = displayName;

    /// <summary>
    /// 是否分层，如地区
    /// </summary>
    public bool IsHierarchical { get; init; }
}
