using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Testing.Contracts;

/// <summary>
/// 集成消息的契约快照
/// </summary>
/// <remarks>
/// 每个消息一个 JSON Schema 文件，进版本管理。消息在 Outbox、Inbox 里存着，旧格式的消息还要能被新代码读出来，
/// 所以契约的任何改动都要有人明确确认：兼容的改动（新增可空字段）更新快照即可，破坏性的改动要新建高一个版本的消息类
/// </remarks>
public static class MessageContractSnapshots
{
    /// <summary>
    /// 设为 1 时，检查会把快照文件改成当前契约，而不是报告差异
    /// </summary>
    public const string UpdateVariable = "KMIC_UPDATE_SNAPSHOTS";

    private static readonly JsonSerializerOptions _writing = new() { WriteIndented = true, NewLine = "\n" };

    /// <summary>
    /// 从入口程序集出发，沿着 Kmic 程序集的引用找到全部契约程序集
    /// </summary>
    /// <remarks>
    /// 宿主引用了哪些模块，就检查哪些模块的契约，新增模块不用另外登记
    /// </remarks>
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

    /// <summary>
    /// 把契约程序集里每个消息的 Schema 与快照目录对照，返回全部问题；没有问题时返回空列表
    /// </summary>
    /// <param name="contractAssemblies">契约程序集</param>
    /// <param name="directory">快照目录，每个消息一个文件，如 Sample.SpecimenSubmitted.v1.json</param>
    public static IReadOnlyList<string> Check(IEnumerable<Assembly> contractAssemblies, string directory) =>
        Check(contractAssemblies, directory, string.Equals(Environment.GetEnvironmentVariable(UpdateVariable), "1", StringComparison.Ordinal));

    /// <summary>
    /// 同 <see cref="Check(IEnumerable{Assembly}, string)" />，明确指定是更新快照还是对照
    /// </summary>
    /// <param name="contractAssemblies">契约程序集</param>
    /// <param name="directory">快照目录</param>
    /// <param name="update">为真时把快照改成当前契约，没有问题可报</param>
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
                    problems.Add($"{file}：没有快照。新消息要把生成的快照一起提交（设环境变量 {UpdateVariable}=1 重跑测试生成）。");
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
                ? $"{file}：契约有破坏性变化（{string.Join("；", breaking)}）。版本号没变时不能这样改，新建 Version 更高的消息类，订阅方同时处理两个版本。"
                : $"{file}：契约变了，是兼容的变化（新增了可空字段）。确认无误后设环境变量 {UpdateVariable}=1 重跑测试，更新快照并一起提交。");
        }

        foreach (var orphan in Directory.EnumerateFiles(directory, "*.json").Select(Path.GetFileName).OfType<string>().Where(f => !current.ContainsKey(f)))
        {
            if (update)
            {
                File.Delete(Path.Combine(directory, orphan));
            }
            else
            {
                problems.Add($"{orphan}：代码里已经没有这个消息。旧版本消息在所有 Inbox 里处理完才能删除消息类，确认之后设环境变量 {UpdateVariable}=1 重跑测试删掉快照。");
            }
        }

        return problems;
    }

    /// <summary>
    /// 当前代码里每个消息的 Schema，键是快照文件名
    /// </summary>
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

    // 只看旧消息能不能被新代码读出来：属性删除、类型改变、新增必填属性
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
