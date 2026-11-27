using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Presentation.Endpoints;

namespace Kmic.BuildingBlocks.Hosting;

internal sealed record HostModule(IModule Module, ModuleEndpoints Endpoints, KmicEnvironments OnlyIn);
