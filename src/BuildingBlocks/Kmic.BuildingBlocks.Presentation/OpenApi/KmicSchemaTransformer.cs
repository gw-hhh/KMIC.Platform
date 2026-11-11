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
/// <remarks>
/// 强类型 ID 是 UUID 字符串；智能枚举是字符串，取值是全部编码，前端得到字面量联合类型。
/// 智能枚举的显示名先放在架构上，文档级转换器再汇总到 x-kmic-enums
/// </remarks>
internal sealed class KmicSchemaTransformer : IOpenApiSchemaTransformer
{
    public const string EnumLabelsKey = "x-kmic-enum";

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;
        MarkDictionaryField(schema, context.JsonPropertyInfo);
        if (type.IsValueType && typeof(IStronglyTypedId).IsAssignableFrom(type))
        {
            AsString(schema);
            schema.Format = "uuid";
        }
        else if (type == typeof(DateTimeOffset))
        {
            // 带自定义转换器的类型，框架只给出格式，没有类型
            AsString(schema);
            schema.Format = "date-time";
        }
        else if (EnumerationSchemas.IsEnumeration(type))
        {
            var members = EnumerationSchemas.Members(type);
            AsString(schema);
            schema.Enum = [.. members.Select(m => (JsonNode)JsonValue.Create(m.Code)!)];
            schema.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
            schema.Extensions[EnumLabelsKey] = new JsonNodeExtension(new JsonArray([.. members.Select(m => (JsonNode)new JsonObject { ["code"] = m.Code, ["name"] = m.Name })]));
        }

        return Task.CompletedTask;
    }

    // 可空的保留 null 类型；其余描述对象形状的内容都去掉
    private static void AsString(OpenApiSchema schema)
    {
        var nullable = schema.Type is { } existing && existing.HasFlag(JsonSchemaType.Null);
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
