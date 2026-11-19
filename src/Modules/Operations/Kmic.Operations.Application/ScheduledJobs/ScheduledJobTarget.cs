namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 交给资源策略与动作目录的定时任务摘要
/// </summary>
/// <param name="Name">任务名</param>
/// <param name="IsDeclared">任务是否仍在代码中声明</param>
/// <param name="HasOverride">是否存在自定义开关或执行时间</param>
public sealed record ScheduledJobTarget(string Name, bool IsDeclared, bool HasOverride);
