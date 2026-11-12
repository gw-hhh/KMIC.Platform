using Kmic.BuildingBlocks.Infrastructure.Hosting;

namespace Kmic.BuildingBlocks.Hosting;

/// <summary>
/// 本实例的组成：角色和启用的模块
/// </summary>
/// <param name="Roles">角色</param>
/// <param name="Modules">启用的模块名</param>
internal sealed record HostComposition(HostRoles Roles, IReadOnlyList<string> Modules);
