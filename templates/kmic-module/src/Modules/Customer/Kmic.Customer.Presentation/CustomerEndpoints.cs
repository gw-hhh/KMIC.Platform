using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Customer.Contracts;

namespace Kmic.Customer.Presentation;

public sealed class CustomerEndpoints : ModuleEndpoints
{
    public override string Module => CustomerModuleInfo.Name;
}
