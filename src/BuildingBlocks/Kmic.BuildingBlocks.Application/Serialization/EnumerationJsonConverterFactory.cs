using System.Text.Json;
using System.Text.Json.Serialization;
using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.BuildingBlocks.Application.Serialization;

// 按编码字符串序列化与反序列化智能枚举
internal sealed class EnumerationJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert is { IsClass: true, IsAbstract: false } && typeToConvert.IsAssignableTo(typeof(IEnumeration));

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(typeof(EnumerationJsonConverter<>).MakeGenericType(typeToConvert))!;

    private sealed class EnumerationJsonConverter<T> : JsonConverter<T>
        where T : Enumeration<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"{typeof(T).Name} 须为编码字符串。");
            }

            var code = reader.GetString();
            return Enumeration<T>.TryFromCode(code, out var value)
                ? value
                : throw new JsonException($"{typeof(T).Name} 不存在编码为“{code}”的取值。");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.Code);
    }
}
