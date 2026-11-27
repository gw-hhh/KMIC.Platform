using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Configuration.Domain.Dictionaries;

/// <summary>字典类型状态</summary>
public sealed class DictionaryTypeStatus : Enumeration<DictionaryTypeStatus>
{
    /// <summary>代码中仍在声明，允许维护字典项</summary>
    public static readonly DictionaryTypeStatus Active = new("Active", "使用中");

    /// <summary>代码中已移除，仅保留历史数据，不可修改</summary>
    public static readonly DictionaryTypeStatus Retired = new("Retired", "已废弃");

    private DictionaryTypeStatus(string code, string name)
        : base(code, name)
    {
    }
}
