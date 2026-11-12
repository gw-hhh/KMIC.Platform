using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Sample.Contracts;

namespace Kmic.Sample.Presentation;

/// <summary>
/// Sample 模块的端点注册类
/// </summary>
public sealed class SampleEndpoints : ModuleEndpoints
{
    /// <inheritdoc />
    public override string Module => SampleModuleInfo.Name;
}
