namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 不需要幂等
/// </summary>
/// <remarks>
/// Create、Submit、Approve 这类命令默认要求幂等，确实不需要时用它写明原因
/// </remarks>
/// <param name="reason">原因</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NotIdempotentAttribute(string reason) : Attribute
{
    /// <summary>
    /// 原因
    /// </summary>
    public string Reason { get; } = reason;
}
