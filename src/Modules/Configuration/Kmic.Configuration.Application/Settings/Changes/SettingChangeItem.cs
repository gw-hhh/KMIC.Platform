using System.Text.Json;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Configuration.Application.Settings.Changes;

/// <summary>
/// 设置变更历史的一行
/// </summary>
/// <param name="Id">记录 ID</param>
/// <param name="Key">设置键</param>
/// <param name="OldValue">改之前的值；此前用的是默认值时为空</param>
/// <param name="NewValue">改之后的值；恢复默认或清除时为空</param>
/// <param name="Reason">原因</param>
/// <param name="ChangedBy">操作人</param>
/// <param name="ChangedByName">操作人的姓名</param>
/// <param name="ChangedAt">时间</param>
/// <param name="TraceId">链路追踪 ID</param>
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
