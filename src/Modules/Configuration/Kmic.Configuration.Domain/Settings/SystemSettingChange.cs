using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Configuration.Domain.Settings;

/// <summary>
/// 设置的一次变更：谁在什么时候把什么改成了什么，为什么
/// </summary>
/// <remarks>
/// 只追加，不改不删：表上对运行账号撤销了修改与删除权限。谁改的、什么时候改的是创建人和创建时间。
/// 自己就是一个聚合：它与被改的设置值不在同一个聚合里，设置值恢复默认后行被删掉，历史还在。
/// 新旧值原样保存，所以设置里不放密钥
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
    /// 改之前的值；此前用的是默认值时为空
    /// </summary>
    public string? OldValue { get; private set; }

    /// <summary>
    /// 改之后的值；恢复默认时为空
    /// </summary>
    public string? NewValue { get; private set; }

    /// <summary>
    /// 原因，必填
    /// </summary>
    public string Reason { get; private set; }

    /// <summary>
    /// 链路追踪 ID，可以到日志与链路里查这次修改
    /// </summary>
    public string? TraceId { get; private set; }

    /// <summary>
    /// 记一次变更
    /// </summary>
    public static SystemSettingChange Record(string key, string? oldValue, string? newValue, string reason, string? traceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        return new SystemSettingChange(SettingChangeId.New(), key, oldValue, newValue, reason, traceId);
    }
}
