using System.Text.Json;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Configuration.Application.Settings.Changes;

/// <summary>设置变更记录</summary>
/// <param name="OldValue">修改前保存的值，原无保存记录时为空</param>
/// <param name="NewValue">修改后保存的值，恢复默认或清除时为空</param>
public sealed record SettingChangeItem(
    Guid Id,
    string Key,
    JsonElement? OldValue,
    JsonElement? NewValue,
    string Reason,
    UserId ChangedBy,
    string? ChangedByName,
    DateTimeOffset ChangedAt,
    string? TraceId);
