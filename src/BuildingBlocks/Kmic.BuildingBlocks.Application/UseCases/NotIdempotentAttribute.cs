namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 按命名约定本应幂等（Create*、Submit*、Approve*、Pay*…），但确实不需要时写明原因
/// </summary>
/// <param name="reason">不需要幂等的原因</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NotIdempotentAttribute(string reason) : Attribute
{
    /// <summary>
    /// 不需要幂等的原因
    /// </summary>
    public string Reason { get; } = reason;
}
