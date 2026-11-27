using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.BuildingBlocks.Application.Auditing;

/// <summary>实体状态流转历史条目。</summary>
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
