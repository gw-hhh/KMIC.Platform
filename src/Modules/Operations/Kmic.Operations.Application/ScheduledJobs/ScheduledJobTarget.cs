namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 交给资源策略与动作目录的定时任务摘要
/// </summary>
/// <param name="Name">任务名</param>
/// <param name="IsDeclared">代码里还有没有这个任务；已经删除的只剩覆盖设置，不能再操作</param>
/// <param name="HasOverride">有没有改过开关或执行时间</param>
public sealed record ScheduledJobTarget(string Name, bool IsDeclared, bool HasOverride);
