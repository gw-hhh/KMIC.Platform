using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Configuration.Domain.Dictionaries;

/// <summary>
/// 字典项的状态
/// </summary>
public sealed class DictionaryItemStatus : Enumeration<DictionaryItemStatus>
{
    /// <summary>
    /// 可以选用
    /// </summary>
    public static readonly DictionaryItemStatus Active = new("Active", "启用");

    /// <summary>
    /// 历史数据仍然显示它，新数据不能再选
    /// </summary>
    public static readonly DictionaryItemStatus Disabled = new("Disabled", "已停用");

    private DictionaryItemStatus(string code, string name)
        : base(code, name)
    {
    }
}
