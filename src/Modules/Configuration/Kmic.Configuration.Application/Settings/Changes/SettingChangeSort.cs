using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Configuration.Application.Settings.Changes;

/// <summary>设置变更历史排序规则（默认按时间倒序）</summary>
public static class SettingChangeSort
{
    public static readonly SortMap<SettingChangeRow> Map = SortMap<SettingChangeRow>
        .Field("changedAt", x => x.ChangedAt)
        .Field("key", x => x.Key)
        .Default("-changedAt");
}
