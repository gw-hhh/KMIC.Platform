using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Sample.Domain.Specimens;

/// <summary>示例的可用操作</summary>
public sealed class SpecimenAction : Enumeration<SpecimenAction>
{
    public static readonly SpecimenAction Submit = new("Submit", "提交");

    public static readonly SpecimenAction Close = new("Close", "关闭");

    /// <summary>增删附件，不引起状态流转</summary>
    public static readonly SpecimenAction Edit = new("Edit", "编辑");

    private SpecimenAction(string code, string name)
        : base(code, name)
    {
    }
}
