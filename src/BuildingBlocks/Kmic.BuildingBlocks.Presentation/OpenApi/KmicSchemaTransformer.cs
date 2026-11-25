using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Kmic.BuildingBlocks.Application.Dictionaries;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Kmic.BuildingBlocks.Presentation.OpenApi;

/// <summary>
/// 强类型 ID 和智能枚举在文档里按它们实际的 JSON 形式描述
/// </summary>
internal sealed class KmicSchemaTransformer : IOpenApiSchemaTransformer
{
    public const string EnumLabelsKey = "x-kmic-enum";

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var underlying = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type);
        var type = underlying ?? context.JsonTypeInfo.Type;
        // 带自定义转换器的值类型，框架不给类型，可空只能从 Nullable<T> 看出来
        var nullable = underlying is not null || (schema.Type is { } existing && existing.HasFlag(JsonSchemaType.Null));
        MarkDictionaryField(schema, context.JsonPropertyInfo);
        if (type.IsValueType && typeof(IStronglyTypedId).IsAssignableFrom(type))
        {
            AsString(schema, nullable);
            schema.Format = "uuid";
        }
        else if (type == typeof(DateTimeOffset))
        {
            // 带自定义转换器的类型，框架只给出格式，没有类型
            AsString(schema, nullable);
            schema.Format = "date-time";
        }
        else if (EnumerationSchemas.IsEnumeration(type))
        {
            var members = EnumerationSchemas.Members(type);
            AsString(schema, nullable);
            schema.Enum = [.. members.Select(m => (JsonNode)JsonValue.Create(m.Code)!)];
            schema.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
            schema.Extensions[EnumLabelsKey] = new JsonNodeExtension(new JsonArray([.. members.Select(m => (JsonNode)new JsonObject { ["code"] = m.Code, ["name"] = m.Name })]));
        }

        return Task.CompletedTask;
    }

    // 其余描述对象形状的内容都去掉
    private static void AsString(OpenApiSchema schema, bool nullable)
    {
        schema.Type = nullable ? JsonSchemaType.String | JsonSchemaType.Null : JsonSchemaType.String;
        schema.Properties?.Clear();
        schema.Required?.Clear();
        schema.AdditionalProperties = null;
        schema.Items = null;
    }

    // 存字典编码的属性标了 [Dictionary]，前端据此把编码显示成字典里的名称。位置参数上的特性在构造函数参数里，也要看
    private static void MarkDictionaryField(OpenApiSchema schema, JsonPropertyInfo? property)
    {
        var declared = property?.AttributeProvider?.GetCustomAttributes(typeof(DictionaryAttribute), inherit: false)
            .Concat(property.AssociatedParameter?.AttributeProvider?.GetCustomAttributes(typeof(DictionaryAttribute), inherit: false) ?? [])
            .OfType<DictionaryAttribute>()
            .FirstOrDefault();
        if (declared is null)
        {
            return;
        }

        schema.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
        schema.Extensions["x-dictionary"] = new JsonNodeExtension(JsonValue.Create(declared.Type));
    }
}
