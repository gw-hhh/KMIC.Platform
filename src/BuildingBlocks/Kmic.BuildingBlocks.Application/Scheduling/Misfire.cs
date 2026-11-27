namespace Kmic.BuildingBlocks.Application.Scheduling;

public enum Misfire
{
    /// <summary>恢复后立即补执行一次，随后按原计划继续。</summary>
    FireOnceNow,

    /// <summary>跳过已错过的触发，等待下一次计划时刻。</summary>
    Skip,
}
