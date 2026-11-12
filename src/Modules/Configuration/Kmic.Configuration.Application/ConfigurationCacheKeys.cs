using Kmic.BuildingBlocks.Application.Caching;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Configuration.Application;

/// <summary>
/// Configuration 模块的缓存键与标签
/// </summary>
public static class ConfigurationCacheKeys
{
    /// <summary>
    /// 全部字典的标签
    /// </summary>
    public const string DictionaryTag = "Configuration:Dictionary";

    /// <summary>
    /// 全部系统设置的标签：被修改过的值都在同一个缓存项里，任何一个设置变了就整体失效
    /// </summary>
    public const string SettingsTag = "Configuration:Settings";

    /// <summary>
    /// 偏好系统层的缓存键，同时也是失效它的标签
    /// </summary>
    public static string PreferencesSystem => CacheKey.Of("Configuration", "Preferences", "System");

    /// <summary>
    /// 偏好某个角色层的缓存键，同时也是只失效这个角色的标签
    /// </summary>
    public static string PreferencesRole(RoleId roleId) => CacheKey.Of("Configuration", "Preferences", "Role", roleId.Value);

    /// <summary>
    /// 一个字典类型的快照的键，同时也是只失效这一个类型的标签
    /// </summary>
    public static string Dictionary(string typeCode) => CacheKey.Of("Configuration", "Dictionary", typeCode);
}
