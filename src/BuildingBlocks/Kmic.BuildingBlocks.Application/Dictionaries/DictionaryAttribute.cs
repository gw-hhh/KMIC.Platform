namespace Kmic.BuildingBlocks.Application.Dictionaries;

/// <summary>
/// 字典字段
/// </summary>
/// <remarks>
/// 标在存字典编码的请求、响应属性上，前端按它显示字典名称
/// </remarks>
/// <param name="type">字典类型编码，如 SampleDictionaries.Industry</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class DictionaryAttribute(string type) : Attribute
{
    /// <summary>
    /// 字典类型编码
    /// </summary>
    public string Type { get; } = type;
}
