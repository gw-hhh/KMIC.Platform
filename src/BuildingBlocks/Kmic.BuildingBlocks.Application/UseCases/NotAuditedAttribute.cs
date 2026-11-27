namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>声明用例无需记录审计并说明原因。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NotAuditedAttribute(string reason) : Attribute
{
    public string Reason { get; } = reason;
}
