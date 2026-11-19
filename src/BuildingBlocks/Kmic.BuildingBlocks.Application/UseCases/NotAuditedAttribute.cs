namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 不记录审计
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NotAuditedAttribute(string reason) : Attribute
{
    /// <summary>
    /// 原因
    /// </summary>
    public string Reason { get; } = reason;
}
