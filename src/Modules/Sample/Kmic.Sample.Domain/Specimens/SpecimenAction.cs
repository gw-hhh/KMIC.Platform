using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Sample.Domain.Specimens;

/// <summary>
/// 示例标本上会引起状态流转的动作
/// </summary>
public sealed class SpecimenAction : Enumeration<SpecimenAction>
{
    /// <summary>
    /// 提交
    /// </summary>
    public static readonly SpecimenAction Submit = new("Submit", "提交");

    /// <summary>
    /// 关闭
    /// </summary>
    public static readonly SpecimenAction Close = new("Close", "关闭");

    private SpecimenAction(string code, string name)
        : base(code, name)
    {
    }
}
