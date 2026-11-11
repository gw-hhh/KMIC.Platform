namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 错过触发时的处理方式
/// </summary>
/// <remarks>
/// 不提供“错过几次补几次”：命令按计划时刻可重复执行，补一次就够了
/// </remarks>
public enum Misfire
{
    /// <summary>
    /// 恢复后马上补执行一次，再按原计划继续
    /// </summary>
    FireOnceNow,

    /// <summary>
    /// 跳过错过的，等下一次计划时刻
    /// </summary>
    Skip,
}
