using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Contracts.Jobs;

/// <summary>用户提交的后台作业完成事件（系统自动提交的作业不发布此事件）。</summary>
[IntegrationMessage("Framework.BackgroundJobFinished")]
public sealed record BackgroundJobFinished : IIntegrationEvent
{
    /// <summary>作业 ID。</summary>
    public required Guid JobId { get; init; }

    /// <summary>所属模块。</summary>
    public required string Module { get; init; }

    /// <summary>作业类型名称。</summary>
    public required string JobType { get; init; }

    /// <summary>作业显示名称。</summary>
    public required string JobName { get; init; }

    /// <summary>执行结果。</summary>
    public required BackgroundJobResult Result { get; init; }

    /// <summary>失败时的错误码。</summary>
    public string? ErrorCode { get; init; }

    /// <summary>失败时的错误说明。</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>结果文件 ID，无结果文件时为 null。</summary>
    public Guid? ResultFileId { get; init; }

    /// <summary>提交用户 ID。</summary>
    public required Guid SubmittedBy { get; init; }

    /// <summary>取消请求用户 ID，未请求取消时为 null。</summary>
    public Guid? CancelRequestedBy { get; init; }

    /// <summary>结束时间。</summary>
    public required DateTimeOffset FinishedAt { get; init; }
}
