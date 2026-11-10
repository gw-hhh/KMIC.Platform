using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 代码中声明、值存在数据库中的一项配置；Configuration 模块经它取默认值、校验管理员写入的值
/// </summary>
public abstract class DeclaredValueDefinition
{
    private readonly Func<object, Result> _validate;

    private protected DeclaredValueDefinition(string key, string displayName, Type valueType, object defaultValue, Func<object, Result> validate)
    {
        Key = key;
        Module = ModuleNames.Of(valueType);
        DisplayName = displayName;
        ValueType = valueType;
        DefaultValue = defaultValue;
        _validate = validate;
    }

    /// <summary>
    /// 键
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 所属模块
    /// </summary>
    public string Module { get; }

    /// <summary>
    /// 显示名
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 值类型
    /// </summary>
    public Type ValueType { get; }

    /// <summary>
    /// 默认值
    /// </summary>
    public object DefaultValue { get; }

    /// <summary>
    /// 执行值类型的 Validate
    /// </summary>
    public Result Validate(object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.GetType() == ValueType
            ? _validate(value)
            : throw new ArgumentException($"{Key} 的值类型应为 {ValueType.Name}，实际为 {value.GetType().Name}。", nameof(value));
    }
}
