using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Configuration.Domain.Settings;

/// <summary>
/// 被修改过的系统设置值：一个设置一行；没有这一行的设置用代码里的默认值
/// </summary>
/// <remarks>
/// 值的校验不在聚合里：校验函数是设置类型自己的静态成员，由命令处理器按设置目录取到并执行，这里只存已经校验过的 JSON
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
    /// 第一次修改某个设置：此前它用代码里的默认值
    /// </summary>
    public static SystemSettingValue Create(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new SystemSettingValue(SystemSettingValueId.New(), key, value);
    }

    /// <summary>
    /// 换成新的值
    /// </summary>
    public void Set(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value;
    }
}
