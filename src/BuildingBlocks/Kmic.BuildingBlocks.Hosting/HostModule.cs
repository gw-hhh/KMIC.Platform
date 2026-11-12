using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Presentation.Endpoints;

namespace Kmic.BuildingBlocks.Hosting;

/// <summary>
/// 宿主里注册的一个模块：服务端注册类和端点注册类配成一对
/// </summary>
internal sealed record HostModule(IModule Module, ModuleEndpoints Endpoints, KmicEnvironments OnlyIn);
