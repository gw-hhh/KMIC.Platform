using System.Globalization;
using System.Text.Json.Nodes;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Kmic.BuildingBlocks.Presentation.OpenApi;

/// <summary>
/// 把端点发送的消息、废弃声明和排序声明写进操作，并补上统一格式的错误响应
/// </summary>
internal sealed class KmicOperationTransformer : IOpenApiOperationTransformer
{
    public const string ProblemSchemaName = "KmicProblem";
    public const string IdempotencyKeyHeader = "Idempotency-Key";

    // 每个操作都可能出现的错误响应：状态码与说明
    private static readonly (string Status, string Description)[] _problemResponses =
    [
        ("400", "请求格式不正确，或者提交的数据没有通过校验，具体见 errors"),
        ("401", "没有登录，或者登录已失效"),
        ("403", "没有执行这个操作的权限"),
        ("404", "数据不存在，或者当前用户看不到"),
        ("409", "数据已被他人修改，或者与现有数据冲突，当前版本号见 currentVersion"),
        ("422", "违反了业务规则，原因见 code 和 title"),
        ("429", "请求太频繁，或者系统正忙，按 Retry-After 等待后再试"),
        ("500", "系统出错了，原因没有写在响应里，报障时提供 traceId"),
    ];

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<SendsMetadata>().FirstOrDefault() is not { } sends)
        {
            return Task.CompletedTask;
        }

        var useCase = context.ApplicationServices.GetRequiredService<UseCaseCatalog>().Find(sends.MessageType);
        if (useCase is not null)
        {
            ApplyUseCase(operation, useCase);
        }

        DescribeSuccessResponse(operation, sends.MessageType);
        FixStronglyTypedIdParameters(operation, context);

        if (metadata.OfType<DeprecationMetadata>().FirstOrDefault() is { } deprecation)
        {
            operation.Deprecated = true;
            Extend(operation, "x-sunset", JsonValue.Create(deprecation.Sunset.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        }

        if (metadata.OfType<SortableMetadata>().FirstOrDefault() is { } sortable)
        {
            Extend(operation, "x-sortable-fields", new JsonArray([.. sortable.Fields.Select(f => (JsonNode)JsonValue.Create(f)!)]));
        }

        AddProblemResponses(operation);
        return Task.CompletedTask;
    }

    // 成功响应的类型由 Sends 声明，这里换上说明文字，并补上 201、202 的 Location 头
    private static void DescribeSuccessResponse(OpenApiOperation operation, Type message)
    {
        var (status, _) = MessageResponse.Of(message);
        if (operation.Responses is null || !operation.Responses.TryGetValue(status.ToString(CultureInfo.InvariantCulture), out var response) || response is not OpenApiResponse success)
        {
            return;
        }

        success.Description = status switch
        {
            StatusCodes.Status201Created => "已创建，Location 指向新数据，响应体是新数据的 ID 与版本号",
            StatusCodes.Status202Accepted => "已受理，Location 指向后台作业，响应体是作业 ID",
            StatusCodes.Status204NoContent => "成功，没有响应体",
            _ => "成功",
        };

        if (status is StatusCodes.Status201Created or StatusCodes.Status202Accepted)
        {
            success.Headers = new Dictionary<string, IOpenApiHeader>(StringComparer.Ordinal)
            {
                ["Location"] = new OpenApiHeader
                {
                    Description = status == StatusCodes.Status201Created ? "新数据的地址" : "后台作业的地址",
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                },
            };
        }
    }

    // 强类型 ID 绑定自路由或查询串，框架按 IParsable 生成的是普通字符串，这里补上 UUID 格式
    private static void FixStronglyTypedIdParameters(OpenApiOperation operation, OpenApiOperationTransformerContext context)
    {
        foreach (var description in context.Description.ParameterDescriptions.Where(p => typeof(IStronglyTypedId).IsAssignableFrom(Nullable.GetUnderlyingType(p.Type) ?? p.Type)))
        {
            if (operation.Parameters?.FirstOrDefault(p => string.Equals(p.Name, description.Name, StringComparison.Ordinal)) is OpenApiParameter parameter)
            {
                parameter.Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" };
            }
        }
    }

    private static void ApplyUseCase(OpenApiOperation operation, UseCase useCase)
    {
        if (useCase.Permission is { } permission)
        {
            Extend(operation, "x-permission", JsonValue.Create(permission));
        }

        // 匿名的端点不要求 Bearer：空的安全要求覆盖文档级的默认值
        if (useCase.Requirement == AccessRequirement.Anonymous)
        {
            operation.Security = [];
        }

        if (useCase.Idempotent)
        {
            Extend(operation, "x-idempotent", JsonValue.Create(true));
            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = IdempotencyKeyHeader,
                In = ParameterLocation.Header,
                Required = true,
                Description = "幂等键，1 到 100 个字符，建议用 UUID。用户发起一次提交时生成，超时重试用同一个键，才不会重复执行；同一个键不能用于内容不同的请求",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String, MinLength = 1, MaxLength = 100 },
            });
        }
    }

    private static void AddProblemResponses(OpenApiOperation operation)
    {
        operation.Responses ??= [];
        foreach (var (status, description) in _problemResponses)
        {
            if (operation.Responses.ContainsKey(status))
            {
                continue;
            }

            operation.Responses[status] = new OpenApiResponse
            {
                Description = description,
                Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal)
                {
                    ["application/problem+json"] = new OpenApiMediaType { Schema = new OpenApiSchemaReference(ProblemSchemaName) },
                },
            };
        }
    }

    private static void Extend(OpenApiOperation operation, string key, JsonNode value)
    {
        operation.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
        operation.Extensions[key] = new JsonNodeExtension(value);
    }
}
