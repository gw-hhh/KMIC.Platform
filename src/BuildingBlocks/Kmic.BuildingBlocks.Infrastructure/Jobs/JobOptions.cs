namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// 后台作业配置
/// </summary>
public sealed class JobOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string Section = "Kmic:Jobs";

    /// <summary>
    /// 每个 Worker 实例同时运行的作业数
    /// </summary>
    public int Parallelism { get; set; } = 2;

    /// <summary>
    /// 领取后的占用时长，执行中每过三分之一续期一次
    /// </summary>
    public TimeSpan Lease { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// 单个作业的最长运行时间
    /// </summary>
    public TimeSpan MaxRunTime { get; set; } = TimeSpan.FromHours(2);

    /// <summary>
    /// 作业运行超过这个时长后，每次续期都查一下提交人是否已被禁用
    /// </summary>
    public TimeSpan SubmitterCheckAfter { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// 可续跑的作业中断后最多续跑的次数
    /// </summary>
    public int MaxResumeAttempts { get; set; } = 3;

    /// <summary>
    /// 作业结束后记录保留的时长
    /// </summary>
    public TimeSpan ResultRetention { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// 进度写入的最短间隔
    /// </summary>
    public TimeSpan ProgressInterval { get; set; } = TimeSpan.FromSeconds(1);
}
