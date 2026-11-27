namespace Kmic.BuildingBlocks.Application.Dictionaries;

/// <summary>声明字典类型的显示名称与层级特性。</summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class DictionaryTypeAttribute(string displayName) : Attribute
{
    public string DisplayName { get; } = displayName;

    /// <summary>是否支持层级结构（如地区）。</summary>
    public bool IsHierarchical { get; init; }
}
