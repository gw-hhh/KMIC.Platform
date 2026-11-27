namespace Kmic.BuildingBlocks.Infrastructure.History;

internal sealed class StateHistoryRecord
{
    public Guid Id { get; set; }

    public Guid AggregateId { get; set; }

    public string StateName { get; set; } = "";

    // 创建时的首次记录为空
    public string? FromCode { get; set; }

    public string ToCode { get; set; } = "";

    public string ActionCode { get; set; } = "";

    public string? Reason { get; set; }

    public int? Revision { get; set; }

    // 系统身份时为空
    public Guid? ActorUserId { get; set; }

    // 模拟登录时的实际操作人
    public Guid? ActualUserId { get; set; }

    public Guid? RoleId { get; set; }

    // 系统身份代表的原始操作人
    public Guid? OnBehalfOfUserId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public string TraceId { get; set; } = "";
}
