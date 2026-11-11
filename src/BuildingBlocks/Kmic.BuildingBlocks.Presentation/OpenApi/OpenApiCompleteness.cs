using System.Text.Json;

namespace Kmic.BuildingBlocks.Presentation.OpenApi;

/// <summary>
/// 生成出来的 OpenAPI 文档的完整性检查
/// </summary>
/// <remarks>
/// 前端看到的接口说明全部来自文档：缺了就是前端看不懂。CI 在构建时生成文档后运行这个检查，缺失即失败。
/// 检查的是文字内容，格式上的问题由架构名唯一等生成时的校验拦截
/// </remarks>
public static class OpenApiCompleteness
{
    private static readonly string[] _methods = ["get", "put", "post", "delete", "options", "head", "patch", "trace"];

    /// <summary>
    /// 返回全部问题，没有问题时为空
    /// </summary>
    /// <param name="document">已解析的 OpenAPI 文档</param>
    public static IReadOnlyList<string> Check(JsonElement document)
    {
        var problems = new List<string>();
        CheckOperations(document, problems);
        CheckSchemas(document, problems);
        CheckEnumLabels(document, problems);
        return problems;
    }

    private static void CheckOperations(JsonElement document, List<string> problems)
    {
        if (!document.TryGetProperty("paths", out var paths))
        {
            return;
        }

        foreach (var path in paths.EnumerateObject())
        {
            foreach (var operation in path.Value.EnumerateObject().Where(o => _methods.Contains(o.Name, StringComparer.Ordinal)))
            {
                var name = $"{operation.Name.ToUpperInvariant()} {path.Name}";
                if (IsBlank(operation.Value, "summary"))
                {
                    problems.Add($"操作 {name} 没有摘要（summary）。");
                }

                if (operation.Value.TryGetProperty("parameters", out var parameters))
                {
                    foreach (var parameter in parameters.EnumerateArray().Where(p => IsBlank(p, "description")))
                    {
                        problems.Add($"操作 {name} 的参数 {parameter.GetProperty("name").GetString()} 没有说明。");
                    }
                }
            }
        }
    }

    // 请求与响应里出现的每个字段都要有说明；字段说明来自 XML 注释
    private static void CheckSchemas(JsonElement document, List<string> problems)
    {
        if (!document.TryGetProperty("components", out var components) || !components.TryGetProperty("schemas", out var schemas))
        {
            return;
        }

        foreach (var schema in schemas.EnumerateObject())
        {
            if (!schema.Value.TryGetProperty("properties", out var properties))
            {
                continue;
            }

            foreach (var property in properties.EnumerateObject().Where(p => IsBlank(p.Value, "description")))
            {
                problems.Add($"架构 {schema.Name} 的字段 {property.Name} 没有说明。");
            }
        }
    }

    // 智能枚举的每个值都要有显示名，并且和架构里的取值一致
    private static void CheckEnumLabels(JsonElement document, List<string> problems)
    {
        if (!document.TryGetProperty("x-kmic-enums", out var labels))
        {
            problems.Add("文档没有 x-kmic-enums，前端生成不出枚举的显示名。");
            return;
        }

        foreach (var entry in labels.EnumerateObject())
        {
            var names = entry.Value.EnumerateArray().ToDictionary(v => v.GetProperty("code").GetString() ?? "", v => v.GetProperty("name").GetString(), StringComparer.Ordinal);
            foreach (var blank in names.Where(n => string.IsNullOrWhiteSpace(n.Value)))
            {
                problems.Add($"智能枚举 {entry.Name} 的取值 {blank.Key} 没有显示名。");
            }

            if (TryGetSchemaCodes(document, entry.Name) is { } codes && !codes.SetEquals(names.Keys))
            {
                problems.Add($"智能枚举 {entry.Name} 的显示名与架构里的取值不一致。");
            }
        }
    }

    private static HashSet<string>? TryGetSchemaCodes(JsonElement document, string schemaName) =>
        document.TryGetProperty("components", out var components)
        && components.TryGetProperty("schemas", out var schemas)
        && schemas.TryGetProperty(schemaName, out var schema)
        && schema.TryGetProperty("enum", out var values)
            ? [.. values.EnumerateArray().Select(v => v.GetString() ?? "")]
            : null;

    private static bool IsBlank(JsonElement element, string property) =>
        !element.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString());
}
