using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Testing.Contracts;

/// <summary>集成消息契约快照校验，每条消息对应一个 JSON Schema 文件（兼容变更直接更新快照，破坏性变更须新建更高版本消息）。</summary>
public static class MessageContractSnapshots
{
    /// <summary>设为 1 时，校验会将快照文件更新为当前契约而非报告差异。</summary>
    public const string UpdateVariable = "KMIC_UPDATE_SNAPSHOTS";

    private static readonly JsonSerializerOptions _writing = new() { WriteIndented = true, NewLine = "\n" };

    /// <summary>从入口程序集出发，沿 Kmic 引用链收集全部契约程序集。</summary>
    public static IReadOnlyList<Assembly> FindContractAssemblies(Assembly entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var contracts = new List<Assembly> { typeof(IIntegrationMessage).Assembly };
        var pending = new Stack<Assembly>([entry]);
        while (pending.TryPop(out var assembly))
        {
            foreach (var reference in assembly.GetReferencedAssemblies().Where(r => r.Name?.StartsWith("Kmic.", StringComparison.Ordinal) == true))
            {
                if (!seen.Add(reference.FullName))
                {
                    continue;
                }

                var loaded = Assembly.Load(reference);
                pending.Push(loaded);
                if (reference.Name!.EndsWith(".Contracts", StringComparison.Ordinal) && !contracts.Contains(loaded))
                {
                    contracts.Add(loaded);
                }
            }
        }

        return contracts;
    }

    /// <summary>将契约消息 Schema 与快照目录比对，返回差异列表。</summary>
    public static IReadOnlyList<string> Check(IEnumerable<Assembly> contractAssemblies, string directory) =>
        Check(contractAssemblies, directory, string.Equals(Environment.GetEnvironmentVariable(UpdateVariable), "1", StringComparison.Ordinal));

    /// <summary>同 <see cref="Check(IEnumerable{Assembly}, string)" />，显式指定更新快照或仅作比对。</summary>
    public static IReadOnlyList<string> Check(IEnumerable<Assembly> contractAssemblies, string directory, bool update)
    {
        ArgumentNullException.ThrowIfNull(contractAssemblies);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        var current = Current(contractAssemblies);
        var problems = new List<string>();
        Directory.CreateDirectory(directory);

        foreach (var (file, schema) in current)
        {
            var path = Path.Combine(directory, file);
            var text = Format(schema);
            if (!File.Exists(path))
            {
                if (update)
                {
                    File.WriteAllText(path, text);
                }
                else
                {
                    problems.Add($"{file}：没有快照。新增消息须一并提交生成的快照（设置环境变量 {UpdateVariable}=1 后重新运行测试生成）。");
                }

                continue;
            }

            var saved = JsonNode.Parse(File.ReadAllText(path))!;
            if (JsonNode.DeepEquals(saved, schema))
            {
                continue;
            }

            if (update)
            {
                File.WriteAllText(path, text);
                continue;
            }

            var breaking = BreakingChanges(saved, schema, "$").ToList();
            problems.Add(breaking.Count > 0
                ? $"{file}：契约存在破坏性变更（{string.Join("；", breaking)}）。版本号未升级时不得作此修改，请新建更高 Version 的消息类，并由订阅方同时兼容两个版本。"
                : $"{file}：契约发生兼容变更（新增可空字段）。确认无误后设置环境变量 {UpdateVariable}=1 重新运行测试，更新快照并一并提交。");
        }

        foreach (var orphan in Directory.EnumerateFiles(directory, "*.json").Select(Path.GetFileName).OfType<string>().Where(f => !current.ContainsKey(f)))
        {
            if (update)
            {
                File.Delete(Path.Combine(directory, orphan));
            }
            else
            {
                problems.Add($"{orphan}：代码中已不存在该消息；须待旧版本消息在所有 Inbox 中处理完成后方可删除消息类，确认后设置环境变量 {UpdateVariable}=1 重新运行测试以清理快照。");
            }
        }

        return problems;
    }

    // 提取当前代码中每条消息的 Schema，键为快照文件名
    private static SortedDictionary<string, JsonNode> Current(IEnumerable<Assembly> contractAssemblies)
    {
        var result = new SortedDictionary<string, JsonNode>(StringComparer.Ordinal);
        var types = contractAssemblies.Distinct().SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false } && t.IsAssignableTo(typeof(IIntegrationMessage)));
        foreach (var type in types)
        {
            if (type.GetCustomAttribute<IntegrationMessageAttribute>() is not { } message)
            {
                continue;
            }

            result.Add($"{message.Name}.v{message.Version}.json", JsonSchemaExporter.GetJsonSchemaAsNode(KmicJsonOptions.Stored, type));
        }

        return result;
    }

    private static string Format(JsonNode schema) => schema.ToJsonString(_writing) + "\n";

    // 仅检查旧消息能否被新代码反序列化：属性删除、类型变更、新增必填属性
    private static IEnumerable<string> BreakingChanges(JsonNode saved, JsonNode current, string path)
    {
        if (saved is not JsonObject oldObject || current is not JsonObject newObject)
        {
            yield break;
        }

        if (!JsonNode.DeepEquals(oldObject["type"], newObject["type"]))
        {
            yield return $"{path} 类型变了";
            yield break;
        }

        var oldRequired = RequiredOf(oldObject);
        var newRequired = RequiredOf(newObject);
        var oldProperties = oldObject["properties"] as JsonObject;
        var newProperties = newObject["properties"] as JsonObject;
        foreach (var (name, oldProperty) in oldProperties ?? [])
        {
            if (newProperties?[name] is not { } newProperty)
            {
                yield return $"{path}.{name} 被删除";
                continue;
            }

            foreach (var change in BreakingChanges(oldProperty!, newProperty, $"{path}.{name}"))
            {
                yield return change;
            }
        }

        foreach (var name in newRequired.Except(oldRequired, StringComparer.Ordinal))
        {
            yield return $"{path}.{name} 变成了必填";
        }

        if (oldObject["items"] is { } oldItems && newObject["items"] is { } newItems)
        {
            foreach (var change in BreakingChanges(oldItems, newItems, $"{path}[]"))
            {
                yield return change;
            }
        }
    }

    private static HashSet<string> RequiredOf(JsonObject schema) =>
        schema["required"] is JsonArray required ? new HashSet<string>(required.Select(r => r!.GetValue<string>()), StringComparer.Ordinal) : new HashSet<string>(StringComparer.Ordinal);
}
