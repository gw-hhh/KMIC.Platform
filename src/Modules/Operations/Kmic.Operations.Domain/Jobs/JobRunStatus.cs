using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Jobs;

/// <summary>
/// 定时任务一次执行的状态
/// </summary>
public sealed class JobRunStatus : Enumeration<JobRunStatus>
{
    /// <summary>
    /// 正在执行
    /// </summary>
    public static readonly JobRunStatus Running = new("Running", "执行中");

    /// <summary>
    /// 成功
    /// </summary>
    public static readonly JobRunStatus Succeeded = new("Succeeded", "成功");

    /// <summary>
    /// 失败
    /// </summary>
    public static readonly JobRunStatus Failed = new("Failed", "失败");

    /// <summary>
    /// 命令提交时出了错，不知道提交了没有：核实后可以手动再执行一次
    /// </summary>
    public static readonly JobRunStatus Unknown = new("Unknown", "结果未知");

    /// <summary>
    /// 执行中进程没了，由清理任务标记
    /// </summary>
    public static readonly JobRunStatus Abandoned = new("Abandoned", "中断");

    private JobRunStatus(string code, string name)
        : base(code, name)
    {
    }
}
