namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// 后台作业
/// </summary>
/// <remarks>
/// 只用来建表和提交，领取、执行、结果写入都是框架里的 SQL
/// </remarks>
internal sealed class BackgroundJobRecord
{
    /// <summary>
    /// 作业 ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 作业名称
    /// </summary>
    public string JobType { get; set; } = "";

    /// <summary>
    /// 参数版本
    /// </summary>
    public int SchemaVersion { get; set; }

    /// <summary>
    /// 作业参数
    /// </summary>
    public string Args { get; set; } = "";

    /// <summary>
    /// 提交人，系统提交时为空
    /// </summary>
    public Guid? SubmittedBy { get; set; }

    /// <summary>
    /// 提交时的当前角色
    /// </summary>
    public Guid? SubmittedRoleId { get; set; }

    /// <summary>
    /// 模拟登录时的实际操作人
    /// </summary>
    public Guid? ActualUserId { get; set; }

    /// <summary>
    /// 提交时间
    /// </summary>
    public DateTimeOffset SubmittedAt { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public short Status { get; set; }

    /// <summary>
    /// 已完成数量
    /// </summary>
    public long ProgressDone { get; set; }

    /// <summary>
    /// 总数量
    /// </summary>
    public long? ProgressTotal { get; set; }

    /// <summary>
    /// 进度说明
    /// </summary>
    public string? ProgressMessage { get; set; }

    /// <summary>
    /// 检查点
    /// </summary>
    public string? Checkpoint { get; set; }

    /// <summary>
    /// 结果文件
    /// </summary>
    public Guid? ResultFileId { get; set; }

    /// <summary>
    /// 结果摘要
    /// </summary>
    public string? ResultSummary { get; set; }

    /// <summary>
    /// 错误码
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 错误说明
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 执行次数
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// 占用令牌
    /// </summary>
    public Guid? LeaseToken { get; set; }

    /// <summary>
    /// 占用到期时间
    /// </summary>
    public DateTimeOffset? LeaseUntil { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTimeOffset? FinishedAt { get; set; }

    /// <summary>
    /// 请求取消的时间
    /// </summary>
    public DateTimeOffset? CancelRequestedAt { get; set; }

    /// <summary>
    /// 请求取消的用户
    /// </summary>
    public Guid? CancelRequestedBy { get; set; }

    /// <summary>
    /// 记录到期时间
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}
