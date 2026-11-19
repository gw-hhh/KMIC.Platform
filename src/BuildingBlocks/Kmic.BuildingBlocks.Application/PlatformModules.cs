using System.Collections.Frozen;

namespace Kmic.BuildingBlocks.Application;

/// <summary>
/// 平台模块：被所有业务模块使用
/// </summary>
internal static class PlatformModules
{
    public static readonly FrozenSet<string> Names = FrozenSet.Create(StringComparer.Ordinal, "Identity", "Operations", "Configuration", "Files", "Notifications");
}
