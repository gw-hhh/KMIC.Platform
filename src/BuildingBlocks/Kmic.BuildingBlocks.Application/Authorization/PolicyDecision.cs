using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 策略判断结果
/// </summary>
/// <param name="Allowed">是否允许</param>
/// <param name="Reason">拒绝原因，允许时为空</param>
/// <param name="Error">拒绝时返回的错误，允许时为空</param>
public readonly record struct PolicyDecision(bool Allowed, DenialReason? Reason, Error? Error)
{
    /// <summary>
    /// 允许
    /// </summary>
    public static PolicyDecision Allow { get; } = new(true, null, null);

    /// <summary>
    /// 拒绝
    /// </summary>
    public static PolicyDecision Deny(DenialReason reason, Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(false, reason, error);
    }
}
