using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Presentation;

public sealed class ConfigurationEndpoints : ModuleEndpoints
{
    public override string Module => ConfigurationModuleInfo.Name;
}
