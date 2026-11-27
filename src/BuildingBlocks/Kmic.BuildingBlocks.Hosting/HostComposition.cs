using Kmic.BuildingBlocks.Infrastructure.Hosting;

namespace Kmic.BuildingBlocks.Hosting;

internal sealed record HostComposition(HostRoles Roles, IReadOnlyList<string> Modules);
