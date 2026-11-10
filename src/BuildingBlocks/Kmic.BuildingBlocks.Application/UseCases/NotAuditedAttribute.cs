namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 命令不写审计事件，必须写明原因
/// </summary>
/// <param name="reason">不审计的原因</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NotAuditedAttribute(string reason) : Attribute
{
    /// <summary>
    /// 不审计的原因
    /// </summary>
    public string Reason { get; } = reason;
}
