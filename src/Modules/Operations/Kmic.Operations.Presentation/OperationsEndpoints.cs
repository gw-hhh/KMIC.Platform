using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Operations.Contracts;

namespace Kmic.Operations.Presentation;

public sealed class OperationsEndpoints : ModuleEndpoints
{
    public override string Module => OperationsModuleInfo.Name;
}
