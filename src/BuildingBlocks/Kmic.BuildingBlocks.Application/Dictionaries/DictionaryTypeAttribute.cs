namespace Kmic.BuildingBlocks.Application.Dictionaries;

/// <summary>
/// 标在字典类型常量上：类型由代码声明，字典项由管理员在界面维护
/// </summary>
/// <param name="displayName">字典管理界面中的名称，如“行业”</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class DictionaryTypeAttribute(string displayName) : Attribute
{
    /// <summary>
    /// 显示名
    /// </summary>
    public string DisplayName { get; } = displayName;

    /// <summary>
    /// 字典项是否分层级（如地区）
    /// </summary>
    public bool IsHierarchical { get; init; }
}
