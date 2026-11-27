namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

internal static class ScheduledRunStatus
{
    public const short Running = 0;

    public const short Succeeded = 1;

    public const short Failed = 2;

    // 提交阶段发生异常，无法确认是否已提交
    public const short Unknown = 3;

    // 执行期间进程意外终止，由后台清理任务标记
    public const short Abandoned = 4;
}
