namespace Kmic.BuildingBlocks.Application.Dictionaries;

/// <summary>
/// 字典类型分组
/// </summary>
/// <param name="module">模块名</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DictionaryTypeGroupAttribute(string module) : Attribute
{
    /// <summary>
    /// 模块名
    /// </summary>
    public string Module { get; } = module;
}
