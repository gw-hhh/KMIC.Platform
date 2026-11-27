using Kmic.BuildingBlocks.Application.Numbering;

namespace Kmic.Sample.Application;

public static class SampleNumberRules
{
    /// <summary>示例编号，按月从 1 递增（格式如 SP-202611-0001）</summary>
    [NumberRule("SP-{yyyy}{MM}-{seq:4}", Reset = NumberReset.Monthly)]
    public const string Specimen = "Sample.Specimen";
}
