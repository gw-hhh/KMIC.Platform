using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kmic.BuildingBlocks.Application.Serialization;

// 统一输出为 UTC，反序列化输入须包含时区偏移
internal sealed class UtcDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 默认读取器会将无偏移时间视为服务器本地时间，此处显式拒绝；含偏移时 TryGetDateTime 返回的 Kind 不为 Unspecified
        if (reader.TokenType != JsonTokenType.String
            || !reader.TryGetDateTimeOffset(out var value)
            || !reader.TryGetDateTime(out var dateTime)
            || dateTime.Kind == DateTimeKind.Unspecified)
        {
            throw new JsonException("时间须为带时区偏移的 ISO 8601 格式，如 2026-11-04T08:30:00Z。");
        }

        return value.ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        // 传入 UtcDateTime 以输出 Z 结尾格式（DateTimeOffset 直接输出会带 +00:00）
        writer.WriteStringValue(value.UtcDateTime);
    }
}
