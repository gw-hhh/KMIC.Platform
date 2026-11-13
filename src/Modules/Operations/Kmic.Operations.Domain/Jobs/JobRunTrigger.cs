using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Jobs;

/// <summary>
/// 定时任务这次执行是怎么触发的
/// </summary>
public sealed class JobRunTrigger : Enumeration<JobRunTrigger>
{
    /// <summary>
    /// 到了计划时间
    /// </summary>
    public static readonly JobRunTrigger Schedule = new("Schedule", "按计划");

    /// <summary>
    /// 管理员点了立即执行
    /// </summary>
    public static readonly JobRunTrigger Manual = new("Manual", "手动执行");

    /// <summary>
    /// 错过了计划时间，事后补执行
    /// </summary>
    public static readonly JobRunTrigger Misfire = new("Misfire", "补执行");

    private JobRunTrigger(string code, string name)
        : base(code, name)
    {
    }
}
