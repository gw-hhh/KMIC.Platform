using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Configuration.Application.Settings;

/// <summary>系统设置操作，供资源策略和可用操作列表使用</summary>
public sealed class SettingAction : Enumeration<SettingAction>
{
    public static readonly SettingAction Edit = new("Edit", "修改");

    public static readonly SettingAction Reset = new("Reset", "恢复默认");

    private SettingAction(string code, string name)
        : base(code, name)
    {
    }
}
