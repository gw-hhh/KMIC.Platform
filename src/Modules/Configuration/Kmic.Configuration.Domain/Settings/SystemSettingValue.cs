using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Configuration.Domain.Settings;

/// <summary>
/// 已保存的系统设置值，未保存时使用代码默认值
/// </summary>
/// <remarks>
/// 值须由调用方按设置目录校验后再写入
/// </remarks>
public sealed class SystemSettingValue : AggregateRoot<SystemSettingValueId>
{
    private SystemSettingValue(SystemSettingValueId id, string key, string value)
        : base(id)
    {
        Key = key;
        Value = value;
    }

    /// <summary>
    /// 设置键，以声明它的模块名开头，如 Sample.Submission
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// 值，JSON 对象
    /// </summary>
    public string Value { get; private set; }

    /// <summary>
    /// 创建设置值记录
    /// </summary>
    public static SystemSettingValue Create(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new SystemSettingValue(SystemSettingValueId.New(), key, value);
    }

    /// <summary>
    /// 更新设置值
    /// </summary>
    public void Set(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value;
    }
}
