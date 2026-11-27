using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Sample.Contracts;

namespace Kmic.Sample.Presentation;

public sealed class SampleEndpoints : ModuleEndpoints
{
    public override string Module => SampleModuleInfo.Name;
}
