using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Configuration.Application.Settings;

/// <summary>
/// 对系统设置的动作：资源策略与列表里的“可用动作”都按它
/// </summary>
public sealed class SettingAction : Enumeration<SettingAction>
{
    /// <summary>
    /// 修改值
    /// </summary>
    public static readonly SettingAction Edit = new("Edit", "修改");

    /// <summary>
    /// 恢复默认值
    /// </summary>
    public static readonly SettingAction Reset = new("Reset", "恢复默认");

    private SettingAction(string code, string name)
        : base(code, name)
    {
    }
}
