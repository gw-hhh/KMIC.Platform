using Kmic.BuildingBlocks.Application.Numbering;

namespace Kmic.Sample.Application;

/// <summary>
/// Sample 模块的编号规则
/// </summary>
public static class SampleNumberRules
{
    /// <summary>
    /// 示例标本编号，每月从 1 开始：SP-202611-0001
    /// </summary>
    [NumberRule("SP-{yyyy}{MM}-{seq:4}", Reset = NumberReset.Monthly)]
    public const string Specimen = "Sample.Specimen";
}
