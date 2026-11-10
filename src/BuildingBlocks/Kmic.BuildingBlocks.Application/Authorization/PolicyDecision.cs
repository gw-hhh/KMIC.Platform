using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 资源策略的判定
/// </summary>
/// <param name="Allowed">是否允许</param>
/// <param name="Reason">拒绝原因，允许时为空</param>
/// <param name="Error">拒绝时返回的错误，来自模块错误目录、类型为 Forbidden；允许时为空</param>
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
