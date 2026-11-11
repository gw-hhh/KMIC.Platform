namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 执行记录的状态，存为 smallint
/// </summary>
internal static class ScheduledRunStatus
{
    public const short Running = 0;

    public const short Succeeded = 1;

    public const short Failed = 2;

    /// <summary>
    /// 命令提交时出错，不知道提交了没有
    /// </summary>
    public const short Unknown = 3;

    /// <summary>
    /// 执行中进程没了，由清理任务标记
    /// </summary>
    public const short Abandoned = 4;
}
