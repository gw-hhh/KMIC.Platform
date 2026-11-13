using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Notifications.Domain.Templates;

/// <summary>
/// 模板的状态
/// </summary>
public sealed class TemplateStatus : Enumeration<TemplateStatus>
{
    /// <summary>
    /// 代码里还有这个模板，可以发送、可以改内容
    /// </summary>
    public static readonly TemplateStatus Active = new("Active", "启用");

    /// <summary>
    /// 代码里已经删掉这个模板：版本记录保留，不能再发送和修改
    /// </summary>
    public static readonly TemplateStatus Retired = new("Retired", "退役");

    private TemplateStatus(string code, string name)
        : base(code, name)
    {
    }
}
