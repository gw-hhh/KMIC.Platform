using System.Collections.Frozen;

namespace Kmic.BuildingBlocks.Application;

/// <summary>
/// 平台模块
/// </summary>
/// <remarks>
/// 被所有业务模块使用的服务。请求型消息只能定义在这些模块里
/// </remarks>
internal static class PlatformModules
{
    public static readonly FrozenSet<string> Names = FrozenSet.Create(StringComparer.Ordinal, "Identity", "Operations", "Configuration", "Files", "Notifications");
}
