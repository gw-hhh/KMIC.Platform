using System.Text.Json;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Application;

/// <summary>
/// 系统设置、偏好这类“代码里声明了值类型”的值，与 JSON 之间的转换和校验
/// </summary>
internal static class DeclaredValues
{
    /// <summary>
    /// 把调用方给的 JSON 读成声明的值类型，再执行声明自己的校验
    /// </summary>
    /// <remarks>
    /// 校验函数是值类型的静态成员，逐字段的错误原样带回去，界面按成员名放回对应的输入框
    /// </remarks>
    /// <param name="definition">设置或偏好的定义</param>
    /// <param name="json">调用方给的值</param>
    /// <param name="invalid">值不合法时的错误</param>
    /// <param name="invalidWith">带逐字段错误的同一个错误</param>
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
            return invalid.WithDetail($"值的格式不对：{exception.Message}");
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

    /// <summary>
    /// 读已经存着的值：存的时候通过了校验，但值类型后来可能改过，读不出来或通不过校验时返回空
    /// </summary>
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

    /// <summary>
    /// 值序列化成存储用的 JSON：按声明的值类型重新序列化，调用方多给的成员不会混进来
    /// </summary>
    public static string ToJson(DeclaredValueDefinition definition, object value)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return JsonSerializer.Serialize(value, definition.ValueType, KmicJsonOptions.Stored);
    }

    /// <summary>
    /// 默认值的 JSON
    /// </summary>
    public static JsonElement DefaultOf(DeclaredValueDefinition definition) => ElementOf(ToJson(definition, definition.DefaultValue));

    /// <summary>
    /// 把存储的 JSON 文本变成可以放进响应的 JSON 元素
    /// </summary>
    public static JsonElement ElementOf(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
