namespace Kmic.BuildingBlocks.Application.Dictionaries;

/// <summary>声明字典类型分组的所属模块。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DictionaryTypeGroupAttribute(string module) : Attribute
{
    public string Module { get; } = module;
}
