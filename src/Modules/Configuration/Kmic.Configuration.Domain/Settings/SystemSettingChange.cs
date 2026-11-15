using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Configuration.Domain.Settings;

/// <summary>
/// 系统设置变更记录
/// </summary>
/// <remarks>
/// 运行账号仅可追加，不可修改或删除；操作人和时间使用创建人、创建时间字段。
/// 与设置值独立保存，恢复默认并删除设置值后仍保留历史。新旧值原样保存，设置中不得包含密钥
/// </remarks>
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
    /// 修改前保存的值
    /// </summary>
    /// <remarks>
    /// 原无保存记录时为空。
    /// </remarks>
    public string? OldValue { get; private set; }

    /// <summary>
    /// 修改后保存的值
    /// </summary>
    /// <remarks>
    /// 恢复默认或清除设置时为空。
    /// </remarks>
    public string? NewValue { get; private set; }

    /// <summary>
    /// 原因，必填
    /// </summary>
    public string Reason { get; private set; }

    /// <summary>
    /// 链路追踪 ID
    /// </summary>
    public string? TraceId { get; private set; }

    /// <summary>
    /// 创建设置变更记录
    /// </summary>
    public static SystemSettingChange Record(string key, string? oldValue, string? newValue, string reason, string? traceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        return new SystemSettingChange(SettingChangeId.New(), key, oldValue, newValue, reason, traceId);
    }
}
