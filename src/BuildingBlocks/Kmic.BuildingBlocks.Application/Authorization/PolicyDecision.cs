using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>资源策略判断结果；允许时 Reason 与 Error 均为 null。</summary>
public readonly record struct PolicyDecision(bool Allowed, DenialReason? Reason, Error? Error)
{
    public static PolicyDecision Allow { get; } = new(true, null, null);

    public static PolicyDecision Deny(DenialReason reason, Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(false, reason, error);
    }
}
