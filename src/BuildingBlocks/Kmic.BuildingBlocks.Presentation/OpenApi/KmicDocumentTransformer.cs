using System.Text.Json.Nodes;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Dictionaries;
using Kmic.BuildingBlocks.Application.Errors;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Kmic.BuildingBlocks.Presentation.OpenApi;

/// <summary>
/// 文档级的内容：Bearer 认证、错误响应的架构、匿名端点清单，以及给前端生成代码用的各种目录
/// </summary>
/// <remarks>
/// 权限点、错误码、字典类型、智能枚举显示名、上传档案都作为文档级扩展输出，前端需要的后端契约
/// 来自同一个构建产物。某个目录没有注册时输出空的，扩展的形状不随模块组成变化
/// </remarks>
internal sealed class KmicDocumentTransformer : IOpenApiDocumentTransformer
{
    public const string BearerScheme = "Bearer";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var services = context.ApplicationServices;
        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal);
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
        document.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);

        document.Components.Schemas[KmicOperationTransformer.ProblemSchemaName] = ProblemSchema();
        document.Components.SecuritySchemes[BearerScheme] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "登录后得到的访问令牌，放在 Authorization 请求头里：Bearer {令牌}",
        };
        document.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(BearerScheme, document)] = [] }];

        Set(document, "x-anonymous-endpoints", AnonymousEndpoints(context, services.GetService<UseCaseCatalog>()));
        Set(document, "x-kmic-permissions", Permissions(services.GetService<PermissionCatalog>()));
        Set(document, "x-kmic-error-codes", ErrorCodes(services.GetService<ErrorCatalog>()));
        Set(document, "x-kmic-dictionary-types", DictionaryTypes(services.GetService<DictionaryTypeCatalog>()));
        Set(document, "x-kmic-enums", EnumLabels(document));

        // 上传档案由文件模块声明，模块就绪之前是空的
        Set(document, "x-kmic-upload-profiles", new JsonObject());
        return Task.CompletedTask;
    }

    private static OpenApiSchema ProblemSchema() => new()
    {
        Type = JsonSchemaType.Object,
        Description = "统一的错误响应，格式是 RFC 9457 的 ProblemDetails 加上几个固定的扩展字段",
        Required = new HashSet<string>(["type", "title", "status", "code", "traceId", "retryable"], StringComparer.Ordinal),
        Properties = new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal)
        {
            ["type"] = Text("错误类型，urn:kmic:error:{code}，与 code 一一对应"),
            ["title"] = Text("给用户看的提示文字"),
            ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32", Description = "HTTP 状态码" },
            ["detail"] = Text("补充说明，没有时不输出"),
            ["instance"] = Text("出错的请求路径，不含查询参数"),
            ["code"] = Text("稳定的错误码，如 Sample.NotOwner；前端按它判断，不按文字判断"),
            ["traceId"] = Text("链路追踪 ID，与响应头 X-Trace-Id 相同，用户报障时提供"),
            ["retryable"] = new OpenApiSchema { Type = JsonSchemaType.Boolean, Description = "原样重试是否可能成功" },
            ["currentVersion"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64", Description = "数据的当前版本号，只在并发冲突时输出" },
            ["errors"] = new OpenApiSchema
            {
                Type = JsonSchemaType.Array,
                Description = "字段错误，校验失败时输出，每条对应请求中的一个字段",
                Items = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    Required = new HashSet<string>(["path", "code", "message"], StringComparer.Ordinal),
                    Properties = new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal)
                    {
                        ["path"] = Text("字段路径，与请求体里的写法一致，如 items[2].quantity"),
                        ["code"] = Text("校验规则，如 Required、Range、Format"),
                        ["message"] = Text("给用户看的提示文字"),
                    },
                },
            },
        },
    };

    private static OpenApiSchema Text(string description) => new() { Type = JsonSchemaType.String, Description = description };

    private static void Set(OpenApiDocument document, string key, JsonNode value) =>
        document.Extensions![key] = new JsonNodeExtension(value);

    // 每个匿名端点一行：方法 + 路径。清单变化必须在代码评审中看得到，所以排序后输出
    private static JsonArray AnonymousEndpoints(OpenApiDocumentTransformerContext context, UseCaseCatalog? catalog)
    {
        var anonymous = context.DescriptionGroups
            .SelectMany(group => group.Items)
            .Where(item => item.ActionDescriptor.EndpointMetadata.OfType<SendsMetadata>().FirstOrDefault() is { } sends
                && catalog?.Find(sends.MessageType) is { Requirement: AccessRequirement.Anonymous })
            .Select(item => $"{item.HttpMethod} /{item.RelativePath}")
            .Order(StringComparer.Ordinal);
        return new JsonArray([.. anonymous.Select(e => (JsonNode)JsonValue.Create(e)!)]);
    }

    private static JsonArray Permissions(PermissionCatalog? catalog) =>
        new([.. (catalog?.All ?? []).OrderBy(p => p.Code, StringComparer.Ordinal).Select(p => (JsonNode)new JsonObject
        {
            ["code"] = p.Code,
            ["name"] = p.DisplayName,
            ["group"] = p.Group,
            ["module"] = p.Module,
            ["sensitive"] = p.Sensitive,
        })]);

    // 框架层的错误（认证、限流、解析等）不经过 Result，不在错误目录里，一并输出
    private static JsonArray ErrorCodes(ErrorCatalog? catalog)
    {
        var business = (catalog?.All ?? []).Select(e => (JsonNode)new JsonObject { ["code"] = e.Code, ["type"] = e.Type.ToString(), ["message"] = e.Message });
        var framework = FrameworkProblems.All.Select(p => (JsonNode)new JsonObject { ["code"] = p.Code, ["type"] = "Framework", ["message"] = p.Title });
        return new JsonArray([.. business.Concat(framework).OrderBy(e => e["code"]!.GetValue<string>(), StringComparer.Ordinal)]);
    }

    private static JsonArray DictionaryTypes(DictionaryTypeCatalog? catalog) =>
        new([.. (catalog?.All ?? []).OrderBy(d => d.Code, StringComparer.Ordinal).Select(d => (JsonNode)new JsonObject
        {
            ["code"] = d.Code,
            ["name"] = d.DisplayName,
            ["module"] = d.Module,
            ["hierarchical"] = d.IsHierarchical,
        })]);

    // 智能枚举的显示名：从架构上取出并汇总成“架构名 → 取值列表”，架构上不留这个临时扩展
    private static JsonObject EnumLabels(OpenApiDocument document)
    {
        var labels = new JsonObject();
        foreach (var (name, schema) in document.Components!.Schemas!.OrderBy(s => s.Key, StringComparer.Ordinal))
        {
            if (schema is OpenApiSchema concrete && concrete.Extensions?.Remove(KmicSchemaTransformer.EnumLabelsKey, out var extension) == true
                && extension is JsonNodeExtension { Node: { } node })
            {
                labels[name] = node.DeepClone();
            }
        }

        return labels;
    }
}
