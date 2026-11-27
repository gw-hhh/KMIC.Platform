namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>后台作业配置</summary>
public sealed class JobOptions
{
    public const string Section = "Kmic:Jobs";

    /// <summary>单个 Worker 实例的并发作业上限</summary>
    public int Parallelism { get; set; } = 2;

    /// <summary>作业租约时长，执行期间每 1/3 时长自动续期</summary>
    public TimeSpan Lease { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>单个作业最长运行时长</summary>
    public TimeSpan MaxRunTime { get; set; } = TimeSpan.FromHours(2);

    /// <summary>运行时长超过此值后，每次续期均复核提交人是否被禁用</summary>
    public TimeSpan SubmitterCheckAfter { get; set; } = TimeSpan.FromHours(1);

    /// <summary>可续跑作业中断后的最大续跑次数</summary>
    public int MaxResumeAttempts { get; set; } = 3;

    /// <summary>作业结束后记录保留时长</summary>
    public TimeSpan ResultRetention { get; set; } = TimeSpan.FromDays(7);

    /// <summary>导出作业允许的最大行数，超出后失败并提示缩小筛选范围</summary>
    public int ExportMaxRows { get; set; } = 100_000;

    /// <summary>进度写入最短间隔</summary>
    public TimeSpan ProgressInterval { get; set; } = TimeSpan.FromSeconds(1);
}
