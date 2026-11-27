namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>声明用例记录操作审计。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AuditedAttribute : Attribute
{
    /// <summary>审计操作名称，命令重命名后需保持原审计名称时指定。</summary>
    public string? Action { get; init; }
}
