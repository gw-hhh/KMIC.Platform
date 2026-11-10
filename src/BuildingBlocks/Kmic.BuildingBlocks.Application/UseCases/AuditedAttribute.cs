namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 命令成功后写审计事件
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AuditedAttribute : Attribute
{
    /// <summary>
    /// 固定的动作名；为空时取“模块.命令名去掉 Command 后缀”，命令改名后要保持连续时填写
    /// </summary>
    public string? Action { get; init; }
}
