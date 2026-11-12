using System.ComponentModel.DataAnnotations;

namespace Kmic.Configuration.Application.Dictionaries;

/// <summary>
/// 字典的部署配置
/// </summary>
public sealed class DictionaryOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Configuration:Dictionary";

    /// <summary>
    /// 一个字典类型最多多少个字典项：整个类型一起加载、一起缓存，所以有上限
    /// </summary>
    [Range(10, 20_000)]
    public int MaxItemsPerType { get; init; } = 2000;
}
