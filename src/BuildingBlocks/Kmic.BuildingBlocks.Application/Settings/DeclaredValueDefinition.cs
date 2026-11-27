using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

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

    public string Key { get; }

    public string Module { get; }

    public string DisplayName { get; }

    public Type ValueType { get; }

    public object DefaultValue { get; }

    public Result Validate(object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.GetType() == ValueType
            ? _validate(value)
            : throw new ArgumentException($"{Key} 的值类型应为 {ValueType.Name}，实际为 {value.GetType().Name}。", nameof(value));
    }
}
