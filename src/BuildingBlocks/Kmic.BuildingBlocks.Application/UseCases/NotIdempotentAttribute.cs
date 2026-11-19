namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 不需要幂等
/// </summary>
/// <remarks>
/// Create、Submit、Approve 这类命令不需要幂等时用它写明原因
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NotIdempotentAttribute(string reason) : Attribute
{
    /// <summary>
    /// 原因
    /// </summary>
    public string Reason { get; } = reason;
}
