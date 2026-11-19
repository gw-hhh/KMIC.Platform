using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Contracts.Jobs;

/// <summary>
/// 用户提交的后台作业已结束
/// </summary>
/// <remarks>
/// 和作业结果在同一个事务里发布；系统提交的作业不发布
/// </remarks>
[IntegrationMessage("Framework.BackgroundJobFinished")]
public sealed record BackgroundJobFinished : IIntegrationEvent
{
    /// <summary>
    /// 作业 ID
    /// </summary>
    public required Guid JobId { get; init; }

    /// <summary>
    /// 作业所在模块
    /// </summary>
    public required string Module { get; init; }

    /// <summary>
    /// 作业类型名
    /// </summary>
    public required string JobType { get; init; }

    /// <summary>
    /// 作业显示名称
    /// </summary>
    public required string JobName { get; init; }

    /// <summary>
    /// 结果
    /// </summary>
    public required BackgroundJobResult Result { get; init; }

    /// <summary>
    /// 失败时的错误码
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// 失败时的错误说明
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 结果文件 ID，没有结果文件时为空
    /// </summary>
    public Guid? ResultFileId { get; init; }

    /// <summary>
    /// 提交人
    /// </summary>
    public required Guid SubmittedBy { get; init; }

    /// <summary>
    /// 请求取消的用户；没有请求取消时为空
    /// </summary>
    public Guid? CancelRequestedBy { get; init; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public required DateTimeOffset FinishedAt { get; init; }
}
