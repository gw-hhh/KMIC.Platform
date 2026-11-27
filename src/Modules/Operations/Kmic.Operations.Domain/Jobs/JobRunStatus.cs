using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Jobs;

/// <summary>定时任务执行状态。</summary>
public sealed class JobRunStatus : Enumeration<JobRunStatus>
{
    public static readonly JobRunStatus Running = new("Running", "执行中");

    public static readonly JobRunStatus Succeeded = new("Succeeded", "成功");

    public static readonly JobRunStatus Failed = new("Failed", "失败");

    /// <summary>无法确认命令是否成功，重试前须核实执行结果。</summary>
    public static readonly JobRunStatus Unknown = new("Unknown", "结果未知");

    /// <summary>执行记录处于运行中且超过清理阈值，由清理任务标为中断。</summary>
    public static readonly JobRunStatus Abandoned = new("Abandoned", "中断");

    private JobRunStatus(string code, string name)
        : base(code, name)
    {
    }
}
