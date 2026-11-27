using Kmic.BuildingBlocks.Presentation.Endpoints;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

internal sealed class ListedModule(string module, params Type[] endpoints) : ModuleEndpoints
{
    public override string Module => module;

    protected override IEnumerable<Type> FindEndpointTypes() => endpoints;
}
