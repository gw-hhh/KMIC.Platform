namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// 修改后的作业行
/// </summary>
/// <param name="Status">状态</param>
/// <param name="JobType">作业类型名</param>
/// <param name="SubmittedBy">提交人，系统提交时为空</param>
/// <param name="CancelRequestedBy">请求取消的用户</param>
/// <param name="ErrorCode">失败时的错误码</param>
/// <param name="ErrorMessage">失败时的错误说明</param>
/// <param name="ResultFileId">结果文件 ID</param>
/// <param name="FinishedAt">结束时间，未结束时为空</param>
internal sealed record UpdatedJob(
    short Status,
    string JobType,
    Guid? SubmittedBy,
    Guid? CancelRequestedBy,
    string? ErrorCode,
    string? ErrorMessage,
    Guid? ResultFileId,
    DateTimeOffset? FinishedAt);
