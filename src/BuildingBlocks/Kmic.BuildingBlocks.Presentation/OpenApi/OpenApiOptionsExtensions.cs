using System.Collections.Concurrent;
using System.Text.Json.Serialization.Metadata;
using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.AspNetCore.OpenApi;

namespace Kmic.BuildingBlocks.Presentation.OpenApi;

/// <summary>
/// OpenAPI 文档的框架转换器
/// </summary>
public static class OpenApiOptionsExtensions
{
    /// <summary>
    /// 加入框架的文档约定：强类型 ID、智能枚举、错误响应、用例声明、目录导出
    /// </summary>
    /// <remarks>
    /// 由宿主在 AddOpenApi 里调用：XML 注释要在调用 AddOpenApi 的项目里才会被读取，所以框架不替宿主调用它
    /// </remarks>
    public static OpenApiOptions AddKmicConventions(this OpenApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // 智能枚举要出现在 components 里（x-kmic-enums 的键用架构名），默认只有对象才会；强类型 ID 就是个 UUID 字符串，不单独成架构。
        // 架构名就是前端生成的类型名，全系统唯一：两个类型同名时生成文档失败，而不是悄悄合并成一个
        var names = new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);
        options.CreateSchemaReferenceId = type =>
        {
            var id = ReferenceId(type);
            return id is null || names.GetOrAdd(id, type.Type) == type.Type
                ? id
                : throw new DeclarationException($"OpenAPI 架构名“{id}”被 {names[id].FullName} 和 {type.Type.FullName} 同时使用。架构名全系统唯一，类名要带上资源名或用例名。");
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

        return typeof(IStronglyTypedId).IsAssignableFrom(Nullable.GetUnderlyingType(type.Type) ?? type.Type)
            ? null
            : OpenApiOptions.CreateDefaultSchemaReferenceId(type);
    }
}
