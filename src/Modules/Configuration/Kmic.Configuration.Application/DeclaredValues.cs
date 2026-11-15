using System.Text.Json;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Application;

/// <summary>
/// 按声明类型转换和校验系统设置、偏好值
/// </summary>
internal static class DeclaredValues
{
    /// <summary>
    /// 按声明的值类型解析 JSON 并校验
    /// </summary>
    /// <remarks>
    /// 调用值类型声明的静态校验函数，原样返回字段错误，供界面定位输入项
    /// </remarks>
    /// <param name="definition">设置或偏好的定义</param>
    /// <param name="json">调用方给的值</param>
    /// <param name="invalid">值不合法时的错误</param>
    /// <param name="invalidWith">将字段校验信息转换为返回错误</param>
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

    /// <summary>
    /// 读取已存储的值，解析或校验失败时返回 null，以兼容值类型后续变更
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
    /// 按声明的值类型序列化，避免保存类型定义之外的成员
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
    /// 将存储的 JSON 文本转换为响应中的 JSON 元素
    /// </summary>
    public static JsonElement ElementOf(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
