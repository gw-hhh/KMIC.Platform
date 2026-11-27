using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Sample.Application;

[DataScopeObjectGroup("Sample")]
public static class SampleScopes
{
    [DataScopeObject("示例")]
    public const string Specimen = "Sample.Specimen";
}
