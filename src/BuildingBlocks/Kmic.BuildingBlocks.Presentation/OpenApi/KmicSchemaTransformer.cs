using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Kmic.BuildingBlocks.Application.Dictionaries;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Kmic.BuildingBlocks.Presentation.OpenApi;

internal sealed class KmicSchemaTransformer : IOpenApiSchemaTransformer
{
    public const string EnumLabelsKey = "x-kmic-enum";

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var underlying = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type);
        var type = underlying ?? context.JsonTypeInfo.Type;
        // 具备自定义 JSON 转换器的值类型不会自动生成 schema.Type，需结合 Nullable<T> 判断可空性
        var nullable = underlying is not null || (schema.Type is { } existing && existing.HasFlag(JsonSchemaType.Null));
        MarkDictionaryField(schema, context.JsonPropertyInfo);
        if (type.IsValueType && typeof(IStronglyTypedId).IsAssignableFrom(type))
        {
            AsString(schema, nullable);
            schema.Format = "uuid";
        }
        else if (type == typeof(DateTimeOffset))
        {
            // 具备自定义转换器的类型仅生成格式而缺失类型声明，需显式补充 string 类型
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

    // 清除用于描述复杂对象结构的残余字段
    private static void AsString(OpenApiSchema schema, bool nullable)
    {
        schema.Type = nullable ? JsonSchemaType.String | JsonSchemaType.Null : JsonSchemaType.String;
        schema.Properties?.Clear();
        schema.Required?.Clear();
        schema.AdditionalProperties = null;
        schema.Items = null;
    }

    // 标记 [Dictionary] 的属性用于前端字典编码翻译；对于主构造函数位置参数，特性位于关联构造函数参数上，需一并读取
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
