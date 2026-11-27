using System.ComponentModel;

namespace Kmic.Operations.Presentation.BackgroundJobs;

/// <param name="Status">状态编码：Queued、Running、Succeeded、Failed、Cancelling、Cancelled</param>
/// <param name="After">上一页返回的游标，首页不传</param>
/// <param name="Limit">每页数量</param>
public sealed record ListMyJobsRequest(string? Status, string? After, [property: DefaultValue(50)] int Limit = 50);
