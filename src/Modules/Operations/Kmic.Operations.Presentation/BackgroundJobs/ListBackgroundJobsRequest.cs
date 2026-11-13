using System.ComponentModel;

namespace Kmic.Operations.Presentation.BackgroundJobs;

/// <summary>
/// 查询后台作业的参数
/// </summary>
/// <param name="Module">只看这个模块的；不给就是全部模块</param>
/// <param name="JobType">作业类型名，如 Sample.ExportSpecimens</param>
/// <param name="Status">状态编码：Queued、Running、Succeeded、Failed、Cancelling、Cancelled</param>
/// <param name="SubmittedBy">提交人</param>
/// <param name="After">上一页返回的游标；第一页不给</param>
/// <param name="Limit">每页数量，1 到 200，默认 50</param>
public sealed record ListBackgroundJobsRequest(
    string? Module,
    string? JobType,
    string? Status,
    Guid? SubmittedBy,
    string? After,
    [property: DefaultValue(50)] int Limit = 50);
