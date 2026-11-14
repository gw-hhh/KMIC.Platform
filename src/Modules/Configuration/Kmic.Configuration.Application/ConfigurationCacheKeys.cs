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
    /// 系统设置缓存键及标签；所有保存值共用一个缓存项，变更后整体失效
    /// </summary>
    public const string SettingsTag = "Configuration:Settings";

    /// <summary>
    /// 系统层偏好的缓存键及失效标签
    /// </summary>
    public static string PreferencesSystem => CacheKey.Of("Configuration", "Preferences", "System");

    /// <summary>
    /// 指定角色层偏好的缓存键及失效标签
    /// </summary>
    public static string PreferencesRole(RoleId roleId) => CacheKey.Of("Configuration", "Preferences", "Role", roleId.Value);

    /// <summary>
    /// 指定字典类型的快照缓存键及失效标签
    /// </summary>
    public static string Dictionary(string typeCode) => CacheKey.Of("Configuration", "Dictionary", typeCode);
}
