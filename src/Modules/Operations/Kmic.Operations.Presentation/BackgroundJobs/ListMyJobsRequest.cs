using System.ComponentModel;

namespace Kmic.Operations.Presentation.BackgroundJobs;

/// <summary>
/// 查询我的后台作业的参数
/// </summary>
/// <param name="Status">状态编码：Queued、Running、Succeeded、Failed、Cancelling、Cancelled</param>
/// <param name="After">上一页返回的游标；第一页不给</param>
/// <param name="Limit">每页数量，1 到 200，默认 50</param>
public sealed record ListMyJobsRequest(string? Status, string? After, [property: DefaultValue(50)] int Limit = 50);
