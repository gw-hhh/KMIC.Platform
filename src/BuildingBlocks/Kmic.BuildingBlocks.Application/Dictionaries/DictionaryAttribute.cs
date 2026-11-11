namespace Kmic.BuildingBlocks.Application.Dictionaries;

/// <summary>
/// 标在存字典编码的请求、响应属性上；接口文档据此标出字典类型，前端按它翻译名称
/// </summary>
/// <param name="type">字典类型编码，如 SampleDictionaries.Industry</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class DictionaryAttribute(string type) : Attribute
{
    /// <summary>
    /// 字典类型编码
    /// </summary>
    public string Type { get; } = type;
}
