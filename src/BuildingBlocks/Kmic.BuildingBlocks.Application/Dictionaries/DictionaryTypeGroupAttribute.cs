namespace Kmic.BuildingBlocks.Application.Dictionaries;

/// <summary>
/// 标在模块声明字典类型的常量类上
/// </summary>
/// <param name="module">模块名，字典类型编码的第一段必须是它</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DictionaryTypeGroupAttribute(string module) : Attribute
{
    /// <summary>
    /// 模块名
    /// </summary>
    public string Module { get; } = module;
}
