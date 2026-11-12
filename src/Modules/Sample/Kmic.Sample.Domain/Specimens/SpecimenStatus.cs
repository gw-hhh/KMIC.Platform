using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Sample.Domain.Specimens;

/// <summary>
/// 示例标本的状态
/// </summary>
public sealed class SpecimenStatus : Enumeration<SpecimenStatus>
{
    /// <summary>
    /// 草稿，可以修改明细
    /// </summary>
    public static readonly SpecimenStatus Draft = new("Draft", "草稿");

    /// <summary>
    /// 已提交
    /// </summary>
    public static readonly SpecimenStatus Submitted = new("Submitted", "已提交");

    /// <summary>
    /// 已关闭
    /// </summary>
    public static readonly SpecimenStatus Closed = new("Closed", "已关闭");

    private SpecimenStatus(string code, string name)
        : base(code, name)
    {
    }
}
