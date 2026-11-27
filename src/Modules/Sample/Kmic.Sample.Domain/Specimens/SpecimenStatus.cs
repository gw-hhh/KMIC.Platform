using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Sample.Domain.Specimens;

/// <summary>示例状态</summary>
public sealed class SpecimenStatus : Enumeration<SpecimenStatus>
{
    public static readonly SpecimenStatus Draft = new("Draft", "草稿");

    public static readonly SpecimenStatus Submitted = new("Submitted", "已提交");

    public static readonly SpecimenStatus Closed = new("Closed", "已关闭");

    private SpecimenStatus(string code, string name)
        : base(code, name)
    {
    }
}
