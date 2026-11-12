using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application.Settings.Changes;

/// <summary>
/// 变更历史按数据库的列读出来的一行，进入排序与分页，再转成列表里的一项
/// </summary>
public sealed record SettingChangeRow(SettingChangeId Id, string Key, string? OldValue, string? NewValue, string Reason, UserId ChangedBy, DateTimeOffset ChangedAt, string? TraceId);
