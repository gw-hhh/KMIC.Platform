namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>声明命令无需幂等校验（如 Create、Submit、Approve 等）并说明原因。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NotIdempotentAttribute(string reason) : Attribute
{
    public string Reason { get; } = reason;
}
