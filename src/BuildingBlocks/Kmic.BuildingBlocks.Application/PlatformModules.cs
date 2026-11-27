using System.Collections.Frozen;

namespace Kmic.BuildingBlocks.Application;

// 平台模块（供所有业务模块使用）
internal static class PlatformModules
{
    public static readonly FrozenSet<string> Names = FrozenSet.Create(StringComparer.Ordinal, "Identity", "Operations", "Configuration", "Files", "Notifications");
}
