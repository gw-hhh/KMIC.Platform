using System.Text.Json;

namespace Kmic.BuildingBlocks.Presentation.OpenApi;

/// <summary>OpenAPI 文档完整性校验器（校验接口摘要与智能枚举显示名）</summary>
public static class OpenApiCompleteness
{
    private static readonly string[] _methods = ["get", "put", "post", "delete", "options", "head", "patch", "trace"];

    /// <summary>校验文档完整性并返回问题列表</summary>
    public static IReadOnlyList<string> Check(JsonElement document)
    {
        var problems = new List<string>();
        CheckOperations(document, problems);
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
                if (IsBlank(operation.Value, "summary"))
                {
                    problems.Add($"操作 {operation.Name.ToUpperInvariant()} {path.Name} 没有摘要（summary）。");
                }
            }
        }
    }

    // 智能枚举的每个取值均须具备非空显示名，且枚举键集合须与 Schema 定义保持一致
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
