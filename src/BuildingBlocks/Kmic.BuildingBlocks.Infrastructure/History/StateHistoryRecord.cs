namespace Kmic.BuildingBlocks.Infrastructure.History;

/// <summary>
/// 一次状态流转的记录
/// </summary>
/// <remarks>
/// 每个有状态的聚合一张表（{聚合}StateHistory），共用这个类。只追加，运行账号改不了
/// </remarks>
internal sealed class StateHistoryRecord
{
    /// <summary>
    /// 记录 ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 聚合根 ID
    /// </summary>
    public Guid AggregateId { get; set; }

    /// <summary>
    /// 状态属性名
    /// </summary>
    public string StateName { get; set; } = "";

    /// <summary>
    /// 原状态编码，创建时为空
    /// </summary>
    public string? FromCode { get; set; }

    /// <summary>
    /// 新状态编码
    /// </summary>
    public string ToCode { get; set; } = "";

    /// <summary>
    /// 动作编码
    /// </summary>
    public string ActionCode { get; set; } = "";

    /// <summary>
    /// 原因
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 业务版本号
    /// </summary>
    public int? Revision { get; set; }

    /// <summary>
    /// 操作人，系统身份时为空
    /// </summary>
    public Guid? ActorUserId { get; set; }

    /// <summary>
    /// 模拟登录时的实际操作人
    /// </summary>
    public Guid? ActualUserId { get; set; }

    /// <summary>
    /// 当时的角色
    /// </summary>
    public Guid? RoleId { get; set; }

    /// <summary>
    /// 系统身份代表的原始操作人
    /// </summary>
    public Guid? OnBehalfOfUserId { get; set; }

    /// <summary>
    /// 发生时间
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>
    /// 追踪 ID
    /// </summary>
    public string TraceId { get; set; } = "";
}
