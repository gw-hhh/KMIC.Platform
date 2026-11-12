using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Configuration.Application;

/// <summary>
/// Configuration 模块的权限点
/// </summary>
/// <remarks>
/// 资源名一律单数。读取字典、读取自己的偏好不需要权限点，登录即可
/// </remarks>
[PermissionGroup("Configuration", "配置")]
public static class ConfigurationPermissions
{
    /// <summary>
    /// 维护字典项
    /// </summary>
    [Permission("维护字典")]
    public const string DictionaryManage = "Configuration.Dictionary.Manage";

    /// <summary>
    /// 查看系统设置与它们的变更历史；修改还要该设置自己声明的权限点
    /// </summary>
    [Permission("查看设置")]
    public const string SettingView = "Configuration.Setting.View";

    /// <summary>
    /// 清除代码里已经不存在的设置留下的值
    /// </summary>
    [Permission("清除不再使用的设置值", Sensitive = true)]
    public const string SettingManage = "Configuration.Setting.Manage";

    /// <summary>
    /// 管理偏好的系统层与角色层；个人层由本人修改，不需要权限点
    /// </summary>
    [Permission("管理偏好的系统与角色默认值")]
    public const string PreferenceManage = "Configuration.Preference.Manage";
}
