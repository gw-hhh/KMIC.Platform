namespace Kmic.BuildingBlocks.Application.Dictionaries;

/// <summary>
/// 字典字段，标在存字典编码的属性上
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class DictionaryAttribute(string type) : Attribute
{
    /// <summary>
    /// 字典类型编码
    /// </summary>
    public string Type { get; } = type;
}
