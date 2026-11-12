using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Configuration.Domain.Dictionaries;

/// <summary>
/// 字典类型的状态
/// </summary>
public sealed class DictionaryTypeStatus : Enumeration<DictionaryTypeStatus>
{
    /// <summary>
    /// 代码里仍然声明着，可以维护字典项
    /// </summary>
    public static readonly DictionaryTypeStatus Active = new("Active", "使用中");

    /// <summary>
    /// 代码里已经删除，数据保留，不能再改
    /// </summary>
    public static readonly DictionaryTypeStatus Retired = new("Retired", "已退役");

    private DictionaryTypeStatus(string code, string name)
        : base(code, name)
    {
    }
}
