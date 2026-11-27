using System.ComponentModel.DataAnnotations;

namespace Kmic.Configuration.Application.Dictionaries;

/// <summary>字典部署配置</summary>
public sealed class DictionaryOptions
{
    public const string Section = "Configuration:Dictionary";

    /// <summary>单个字典类型的字典项数量上限，用于限制加载与缓存规模</summary>
    [Range(10, 20_000)]
    public int MaxItemsPerType { get; init; } = 2000;
}
