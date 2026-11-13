namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 一个后台作业的状态：看得到进度、结果摘要和错误，看不到作业参数与检查点
/// </summary>
/// <param name="Id">作业 ID</param>
/// <param name="Module">作业所在的模块</param>
/// <param name="JobType">作业类型名，如 Sample.ExportSpecimens</param>
/// <param name="State">状态</param>
/// <param name="SubmittedBy">提交人；系统提交的没有</param>
/// <param name="ActualUserId">模拟登录时真正动手的人</param>
/// <param name="SubmittedAt">提交时间</param>
/// <param name="StartedAt">第一次开始执行的时间</param>
/// <param name="FinishedAt">结束时间</param>
/// <param name="ProgressDone">已完成数量</param>
/// <param name="ProgressTotal">总数量，不知道时为空</param>
/// <param name="ProgressMessage">进度说明</param>
/// <param name="ResultFileId">结果文件</param>
/// <param name="ResultSummary">结果摘要，JSON 文本</param>
/// <param name="ErrorCode">失败时的错误码</param>
/// <param name="ErrorMessage">失败时的错误说明</param>
/// <param name="Attempts">执行次数，续跑会加一</param>
/// <param name="CancelRequestedAt">请求取消的时间</param>
/// <param name="CancelRequestedBy">请求取消的人</param>
public sealed record JobView(
    Guid Id,
    string Module,
    string JobType,
    BackgroundJobState State,
    Guid? SubmittedBy,
    Guid? ActualUserId,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    long ProgressDone,
    long? ProgressTotal,
    string? ProgressMessage,
    Guid? ResultFileId,
    string? ResultSummary,
    string? ErrorCode,
    string? ErrorMessage,
    int Attempts,
    DateTimeOffset? CancelRequestedAt,
    Guid? CancelRequestedBy);
