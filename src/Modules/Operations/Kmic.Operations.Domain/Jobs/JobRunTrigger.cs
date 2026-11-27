using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Jobs;

/// <summary>定时任务触发方式。</summary>
public sealed class JobRunTrigger : Enumeration<JobRunTrigger>
{
    public static readonly JobRunTrigger Schedule = new("Schedule", "按计划");

    public static readonly JobRunTrigger Manual = new("Manual", "手动执行");

    /// <summary>错过计划时间后的补执行。</summary>
    public static readonly JobRunTrigger Misfire = new("Misfire", "补执行");

    private JobRunTrigger(string code, string name)
        : base(code, name)
    {
    }
}
