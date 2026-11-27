using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Notifications.Domain.Templates;

/// <summary>模板状态</summary>
public sealed class TemplateStatus : Enumeration<TemplateStatus>
{
    /// <summary>启用：代码中仍声明该模板，允许发送与修改</summary>
    public static readonly TemplateStatus Active = new("Active", "启用");

    /// <summary>已废弃：代码声明已移除，保留历史版本但禁止发送与修改</summary>
    public static readonly TemplateStatus Retired = new("Retired", "已废弃");

    private TemplateStatus(string code, string name)
        : base(code, name)
    {
    }
}
