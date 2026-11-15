using Kmic.Configuration.Domain.Dictionaries;
using Kmic.Configuration.Domain.Preferences;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application;

/// <summary>
/// Configuration 模块的查询入口
/// </summary>
/// <remarks>
/// 查询处理器通过此接口读取，不跟踪实体，也不读取命令中尚未保存的修改。配置数据不声明数据范围，访问由用例的权限要求决定
/// </remarks>
public interface IConfigurationQueryDb
{
    /// <summary>
    /// 字典类型，含字典项
    /// </summary>
    IQueryable<DictionaryType> DictionaryTypes { get; }

    /// <summary>
    /// 已保存的系统设置值
    /// </summary>
    IQueryable<SystemSettingValue> SettingValues { get; }

    /// <summary>
    /// 系统设置的变更历史
    /// </summary>
    IQueryable<SystemSettingChange> SettingChanges { get; }

    /// <summary>
    /// 偏好在各层上的值
    /// </summary>
    IQueryable<PreferenceValue> PreferenceValues { get; }
}
