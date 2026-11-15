using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 定时任务管理动作
/// </summary>
public sealed class ScheduledJobAction : Enumeration<ScheduledJobAction>
{
    /// <summary>
    /// 停用、启用、改执行时间
    /// </summary>
    public static readonly ScheduledJobAction Edit = new("Edit", "修改计划");

    /// <summary>
    /// 恢复代码里的默认值
    /// </summary>
    public static readonly ScheduledJobAction RestoreDefault = new("RestoreDefault", "恢复默认");

    /// <summary>
    /// 手动触发一次
    /// </summary>
    public static readonly ScheduledJobAction RunNow = new("RunNow", "手动触发");

    private ScheduledJobAction(string code, string name)
        : base(code, name)
    {
    }
}
