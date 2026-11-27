using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Configuration.Domain.Settings;

/// <summary>系统设置变更记录（仅追加，禁止在设置值中存放密钥）</summary>
public sealed class SystemSettingChange : AggregateRoot<SettingChangeId>
{
    private SystemSettingChange(SettingChangeId id, string key, string? oldValue, string? newValue, string reason, string? traceId)
        : base(id)
    {
        Key = key;
        OldValue = oldValue;
        NewValue = newValue;
        Reason = reason;
        TraceId = traceId;
    }

    /// <summary>
    /// 设置键
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// 修改前保存的值，原来没有保存记录时为空
    /// </summary>
    public string? OldValue { get; private set; }

    /// <summary>
    /// 修改后保存的值，恢复默认或清除设置时为空
    /// </summary>
    public string? NewValue { get; private set; }

    /// <summary>
    /// 原因，必填
    /// </summary>
    public string Reason { get; private set; }

    /// <summary>
    /// 链路追踪 ID
    /// </summary>
    public string? TraceId { get; private set; }

    public static SystemSettingChange Record(string key, string? oldValue, string? newValue, string reason, string? traceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        return new SystemSettingChange(SettingChangeId.New(), key, oldValue, newValue, reason, traceId);
    }
}
