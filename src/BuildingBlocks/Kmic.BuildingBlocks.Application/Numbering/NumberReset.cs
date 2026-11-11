namespace Kmic.BuildingBlocks.Application.Numbering;

/// <summary>
/// 序号重置周期
/// </summary>
public enum NumberReset
{
    /// <summary>
    /// 不重置
    /// </summary>
    Never,

    /// <summary>
    /// 每年
    /// </summary>
    Yearly,

    /// <summary>
    /// 每月
    /// </summary>
    Monthly,

    /// <summary>
    /// 每天
    /// </summary>
    Daily,
}
