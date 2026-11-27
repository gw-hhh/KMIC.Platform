using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Configuration.Domain.Dictionaries;

/// <summary>字典项状态</summary>
public sealed class DictionaryItemStatus : Enumeration<DictionaryItemStatus>
{
    /// <summary>可正常选用</summary>
    public static readonly DictionaryItemStatus Active = new("Active", "启用");

    /// <summary>仅保留给历史数据展示，新数据不可再选用</summary>
    public static readonly DictionaryItemStatus Disabled = new("Disabled", "已停用");

    private DictionaryItemStatus(string code, string name)
        : base(code, name)
    {
    }
}
