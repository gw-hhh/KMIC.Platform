using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.AspNetCore.OpenApi;

namespace Kmic.BuildingBlocks.Presentation.OpenApi;

public static class OpenApiOptionsExtensions
{
    /// <summary>注册框架 OpenAPI 约定（须由宿主在 AddOpenApi 中直接调用，否则无法加载 XML 注释）</summary>
    public static OpenApiOptions AddKmicConventions(this OpenApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // 智能枚举须独立纳入 components 架构（x-kmic-enums 以架构名为键），强类型 ID 则内联为 UUID 字符串；
        // 架构名同时对应前端生成类型名且需全局唯一，若发生同名冲突则直接拒绝生成文档而非静默合并
        var names = new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);
        options.CreateSchemaReferenceId = type =>
        {
            var id = ReferenceId(type);
            return id is null || names.GetOrAdd(id, type.Type) == type.Type
                ? id
                : throw new DeclarationException($"OpenAPI 架构名“{id}”被 {names[id].FullName} 与 {type.Type.FullName} 重复使用。架构名须全局唯一，请在类名中体现资源名或用例名。");
        };
        options.AddSchemaTransformer(new KmicSchemaTransformer());
        options.AddOperationTransformer(new KmicOperationTransformer());
        options.AddDocumentTransformer(new KmicDocumentTransformer());
        return options;
    }

    private static string? ReferenceId(JsonTypeInfo type)
    {
        if (EnumerationSchemas.IsEnumeration(type.Type))
        {
            return type.Type.Name;
        }

        // 强类型 ID 固定映射为 UUID 字符串，JsonElement 表示任意 JSON 值，二者均直接内联而不生成独立架构；
        // 可空与非空 JsonElement 归为同一底层类型处理，避免因命名冲突导致文档生成失败
        var underlying = Nullable.GetUnderlyingType(type.Type) ?? type.Type;
        return typeof(IStronglyTypedId).IsAssignableFrom(underlying) || underlying == typeof(JsonElement)
            ? null
            : OpenApiOptions.CreateDefaultSchemaReferenceId(type);
    }
}
