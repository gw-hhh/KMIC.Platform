using Kmic.Configuration.Domain.Dictionaries;
using Kmic.Configuration.Domain.Preferences;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application;

/// <summary>Configuration 模块的只读查询入口</summary>
public interface IConfigurationQueryDb
{
    /// <summary>字典类型（含字典项）</summary>
    IQueryable<DictionaryType> DictionaryTypes { get; }

    /// <summary>已保存的系统设置值</summary>
    IQueryable<SystemSettingValue> SettingValues { get; }

    /// <summary>系统设置的变更历史</summary>
    IQueryable<SystemSettingChange> SettingChanges { get; }

    /// <summary>各层偏好值</summary>
    IQueryable<PreferenceValue> PreferenceValues { get; }
}
