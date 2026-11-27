namespace Kmic.BuildingBlocks.Application.Dictionaries;

/// <summary>标注属性或参数关联的字典类型编码。</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class DictionaryAttribute(string type) : Attribute
{
    public string Type { get; } = type;
}
