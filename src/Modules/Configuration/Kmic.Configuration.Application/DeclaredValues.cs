using System.Text.Json;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Application;

// 按声明类型解析、校验与序列化系统设置及偏好值
internal static class DeclaredValues
{
    public static Result<object> Parse(DeclaredValueDefinition definition, JsonElement json, Error invalid, Func<IReadOnlyList<FieldError>, Error> invalidWith)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(invalid);
        ArgumentNullException.ThrowIfNull(invalidWith);

        object? value;
        try
        {
            value = JsonSerializer.Deserialize(json.GetRawText(), definition.ValueType, KmicJsonOptions.Stored);
        }
        catch (JsonException exception)
        {
            return invalid.WithDetail($"值的格式无效：{exception.Message}");
        }

        if (value is null)
        {
            return invalid.WithDetail("值不能为空");
        }

        var validated = definition.Validate(value);
        if (validated.IsFailure)
        {
            return invalidWith(validated.Error.FieldErrors);
        }

        return value;
    }

    // 读取已存储的值，反序列化或校验失败时返回 null 由调用方回退默认值
    public static object? TryRead(DeclaredValueDefinition definition, string json)
    {
        ArgumentNullException.ThrowIfNull(definition);

        try
        {
            var value = JsonSerializer.Deserialize(json, definition.ValueType, KmicJsonOptions.Stored);
            return value is not null && definition.Validate(value).IsSuccess ? value : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // 按声明的值类型序列化，过滤类型定义之外的多余字段
    public static string ToJson(DeclaredValueDefinition definition, object value)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return JsonSerializer.Serialize(value, definition.ValueType, KmicJsonOptions.Stored);
    }

    public static JsonElement DefaultOf(DeclaredValueDefinition definition) => ElementOf(ToJson(definition, definition.DefaultValue));

    public static JsonElement ElementOf(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
