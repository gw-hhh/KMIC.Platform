using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Configuration.Application.Settings.Changes;

/// <summary>
/// 设置变更历史允许排序的字段
/// </summary>
public static class SettingChangeSort
{
    /// <summary>
    /// 排序字段表，默认按时间倒序
    /// </summary>
    public static readonly SortMap<SettingChangeRow> Map = SortMap<SettingChangeRow>
        .Field("changedAt", x => x.ChangedAt)
        .Field("key", x => x.Key)
        .Default("-changedAt");
}
