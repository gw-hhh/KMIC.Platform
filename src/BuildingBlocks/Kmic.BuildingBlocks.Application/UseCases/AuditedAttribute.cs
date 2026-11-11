namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 记录审计
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AuditedAttribute : Attribute
{
    /// <summary>
    /// 操作名，命令改名后要保持原操作名时填写
    /// </summary>
    public string? Action { get; init; }
}
