using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Files.Contracts;

namespace Kmic.Files.Presentation;

public sealed class FilesEndpoints : ModuleEndpoints
{
    public override string Module => FilesModuleInfo.Name;
}
