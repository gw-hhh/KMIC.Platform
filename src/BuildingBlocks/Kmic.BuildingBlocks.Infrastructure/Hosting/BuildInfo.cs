using System.Reflection;

namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

internal static class BuildInfo
{
    public static string Version { get; } =
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
}
