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
    /// 单个字典类型的字典项数量上限，限制整体加载和缓存的规模
    /// </summary>
    [Range(10, 20_000)]
    public int MaxItemsPerType { get; init; } = 2000;
}
