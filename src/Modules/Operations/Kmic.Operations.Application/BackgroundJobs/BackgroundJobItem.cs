using System.Text.Json;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.BackgroundJobs;

/// <summary>
/// 后台作业信息，不含作业参数
/// </summary>
/// <param name="Id">作业 ID</param>
/// <param name="Module">作业所在的模块</param>
/// <param name="JobType">作业类型名</param>
/// <param name="JobName">作业显示名称</param>
/// <param name="Status">状态</param>
/// <param name="SubmittedBy">提交人 ID，系统提交时为空</param>
/// <param name="SubmittedByName">提交人的名称，用户已经不存在时为空</param>
/// <param name="SubmittedAt">提交时间</param>
/// <param name="StartedAt">首次执行时间</param>
/// <param name="FinishedAt">结束时间</param>
/// <param name="ProgressDone">已完成数量</param>
/// <param name="ProgressTotal">总数量，未知时为空</param>
/// <param name="ProgressMessage">进度说明</param>
/// <param name="ResultFileId">结果文件 ID</param>
/// <param name="ResultSummary">结果摘要（JSON）</param>
/// <param name="ErrorCode">失败时的错误码</param>
/// <param name="ErrorMessage">失败时的错误说明</param>
/// <param name="Attempts">执行尝试次数</param>
/// <param name="CancelRequestedAt">取消请求时间</param>
/// <param name="CancelRequestedBy">请求取消的用户 ID</param>
/// <param name="CancelRequestedByName">请求取消的用户名称</param>
public sealed record BackgroundJobItem(
    Guid Id,
    string Module,
    string JobType,
    string JobName,
    BackgroundJobStatus Status,
    Guid? SubmittedBy,
    string? SubmittedByName,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    long ProgressDone,
    long? ProgressTotal,
    string? ProgressMessage,
    Guid? ResultFileId,
    JsonElement? ResultSummary,
    string? ErrorCode,
    string? ErrorMessage,
    int Attempts,
    DateTimeOffset? CancelRequestedAt,
    Guid? CancelRequestedBy,
    string? CancelRequestedByName);
