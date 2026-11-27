using Kmic.BuildingBlocks.Application.Caching;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Configuration.Application;

/// <summary>Configuration 模块的缓存键与失效标签</summary>
public static class ConfigurationCacheKeys
{
    /// <summary>全字典目录变更失效标签</summary>
    public const string DictionaryTag = "Configuration:Dictionary";

    /// <summary>系统设置缓存键及标签（所有保存值共用一个缓存项，变更后整体失效）</summary>
    public const string SettingsTag = "Configuration:Settings";

    /// <summary>系统层偏好缓存键及标签</summary>
    public static string PreferencesSystem => CacheKey.Of("Configuration", "Preferences", "System");

    /// <summary>角色层偏好缓存键及标签</summary>
    public static string PreferencesRole(RoleId roleId) => CacheKey.Of("Configuration", "Preferences", "Role", roleId.Value);

    /// <summary>单个字典快照缓存键及标签</summary>
    public static string Dictionary(string typeCode) => CacheKey.Of("Configuration", "Dictionary", typeCode);
}
