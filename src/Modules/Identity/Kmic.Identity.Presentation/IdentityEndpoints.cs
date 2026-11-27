using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Identity.Contracts;

namespace Kmic.Identity.Presentation;

public sealed class IdentityEndpoints : ModuleEndpoints
{
    public override string Module => IdentityModuleInfo.Name;
}
