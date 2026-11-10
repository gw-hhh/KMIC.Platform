namespace Kmic.BuildingBlocks.Application.Numbering;

/// <summary>
/// 序号的重置周期
/// </summary>
public enum NumberReset
{
    /// <summary>
    /// 不重置
    /// </summary>
    Never,

    /// <summary>
    /// 每年从 1 开始
    /// </summary>
    Yearly,

    /// <summary>
    /// 每月从 1 开始
    /// </summary>
    Monthly,

    /// <summary>
    /// 每天从 1 开始
    /// </summary>
    Daily,
}
