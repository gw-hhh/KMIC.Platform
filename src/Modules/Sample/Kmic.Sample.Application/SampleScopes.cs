using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Sample.Application;

/// <summary>
/// Sample 模块的数据范围对象
/// </summary>
[DataScopeObjectGroup("Sample")]
public static class SampleScopes
{
    /// <summary>
    /// 示例标本
    /// </summary>
    [DataScopeObject("示例标本")]
    public const string Specimen = "Sample.Specimen";
}
