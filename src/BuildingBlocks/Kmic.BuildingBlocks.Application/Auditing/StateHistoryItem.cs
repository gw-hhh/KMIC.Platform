using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.BuildingBlocks.Application.Auditing;

/// <summary>
/// 业务历史中的一次状态流转；人员与角色的显示名在查询时解析
/// </summary>
/// <param name="StateName">状态名，一个聚合可以有多个状态</param>
/// <param name="FromCode">原状态编码，创建时为空</param>
/// <param name="ToCode">新状态编码</param>
/// <param name="ActionCode">动作编码</param>
/// <param name="Reason">原因</param>
/// <param name="Revision">修订号，没有修订概念的聚合为空</param>
/// <param name="Actor">操作人</param>
/// <param name="ActualUser">模拟身份时的实际操作人</param>
/// <param name="Role">操作时的角色</param>
/// <param name="OnBehalfOf">代办时被代办的人</param>
/// <param name="OccurredAt">发生时刻</param>
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
