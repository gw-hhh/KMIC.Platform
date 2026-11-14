using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application.Settings.Changes;

/// <summary>
/// 用于设置变更历史排序和分页的数据库投影
/// </summary>
public sealed record SettingChangeRow(SettingChangeId Id, string Key, string? OldValue, string? NewValue, string Reason, UserId ChangedBy, DateTimeOffset ChangedAt, string? TraceId);
