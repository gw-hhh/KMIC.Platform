using System.Reflection;

namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>
/// 当前构建
/// </summary>
internal static class BuildInfo
{
    /// <summary>
    /// 入口程序集的版本，用于登记和诊断
    /// </summary>
    public static string Version { get; } =
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
}
