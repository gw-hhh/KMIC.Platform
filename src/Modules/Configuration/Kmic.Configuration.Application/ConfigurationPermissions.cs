using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Configuration.Application;

/// <summary>Configuration 模块的权限点（字典查询及个人偏好读写仅需登录即可）</summary>
[PermissionGroup("Configuration", "配置")]
public static class ConfigurationPermissions
{
    /// <summary>维护字典项</summary>
    [Permission("维护字典")]
    public const string DictionaryManage = "Configuration.Dictionary.Manage";

    /// <summary>查看系统设置及变更历史（修改值还须具备该设置声明的权限）</summary>
    [Permission("查看设置")]
    public const string SettingView = "Configuration.Setting.View";

    /// <summary>清除已移除代码声明的残留设置值</summary>
    [Permission("清除不再使用的设置值", Sensitive = true)]
    public const string SettingManage = "Configuration.Setting.Manage";

    /// <summary>管理系统层与角色层偏好（个人层由本人修改，无需权限点）</summary>
    [Permission("管理偏好的系统与角色默认值")]
    public const string PreferenceManage = "Configuration.Preference.Manage";
}
