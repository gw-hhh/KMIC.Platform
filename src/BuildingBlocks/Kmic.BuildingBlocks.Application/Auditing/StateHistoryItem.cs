using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.BuildingBlocks.Application.Auditing;

/// <summary>
/// 业务历史记录
/// </summary>
/// <param name="StateName">状态名</param>
/// <param name="FromCode">原状态编码</param>
/// <param name="ToCode">新状态编码</param>
/// <param name="ActionCode">动作编码</param>
/// <param name="Reason">原因</param>
/// <param name="Revision">业务版本号</param>
/// <param name="Actor">操作人</param>
/// <param name="ActualUser">实际操作人</param>
/// <param name="Role">角色</param>
/// <param name="OnBehalfOf">被代办人</param>
/// <param name="OccurredAt">发生时间</param>
public sealed record StateHistoryItem(
    string StateName,
    string? FromCode,
    string ToCode,
    string ActionCode,
    string? Reason,
    int? Revision,
    ReferenceInfo? Actor,
    ReferenceInfo? ActualUser,
    ReferenceInfo? Role,
    ReferenceInfo? OnBehalfOf,
    DateTimeOffset OccurredAt);
