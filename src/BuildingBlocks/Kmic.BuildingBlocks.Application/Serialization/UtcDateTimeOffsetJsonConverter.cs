using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kmic.BuildingBlocks.Application.Serialization;

/// <summary>
/// 时刻的 JSON 转换，输出 UTC，输入必须带时区偏移
/// </summary>
internal sealed class UtcDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 内置的读取会把不带偏移的时刻当成服务器本地时间，这里拒绝。带偏移时 TryGetDateTime 返回的 Kind 不会是 Unspecified
        if (reader.TokenType != JsonTokenType.String
            || !reader.TryGetDateTimeOffset(out var value)
            || !reader.TryGetDateTime(out var dateTime)
            || dateTime.Kind == DateTimeKind.Unspecified)
        {
            throw new JsonException("时刻要写成带时区偏移的 ISO 8601 格式，如 2026-11-04T08:30:00Z。");
        }

        return value.ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        // UTC 的 DateTime 按 Z 结尾输出，DateTimeOffset 会写成 +00:00
        writer.WriteStringValue(value.UtcDateTime);
    }
}
