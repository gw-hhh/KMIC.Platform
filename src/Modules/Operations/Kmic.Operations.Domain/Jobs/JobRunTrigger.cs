using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Jobs;

/// <summary>
/// 定时任务触发方式
/// </summary>
public sealed class JobRunTrigger : Enumeration<JobRunTrigger>
{
    /// <summary>
    /// 到了计划时间
    /// </summary>
    public static readonly JobRunTrigger Schedule = new("Schedule", "按计划");

    /// <summary>
    /// 通过手动请求触发
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
