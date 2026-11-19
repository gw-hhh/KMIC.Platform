using System.ComponentModel;

namespace Kmic.Operations.Presentation.ScheduledJobs;

/// <summary>
/// 查询定时任务执行记录的参数
/// </summary>
/// <param name="After">上一页返回的游标，首页不传</param>
/// <param name="Limit">每页数量</param>
public sealed record ListScheduledJobRunsRequest(string? After, [property: DefaultValue(50)] int Limit = 50);
