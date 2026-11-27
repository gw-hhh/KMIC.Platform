using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kmic.BuildingBlocks.Analyzers.Generators;

/// <summary>属性中文显示名生成器，供校验提示自动读取（仅扫描 Application 与 Contracts 层，提取属性 summary 或记录 param 的首个短语）。</summary>
[Generator(LanguageNames.CSharp)]
public sealed class DisplayNameGenerator : IIncrementalGenerator
{
    public const string GeneratedTypeName = "Kmic.Generated.DisplayNames";

    // 首短语超过此长度通常为完整说明而非属性名，不予采用
    private const int MaxNameLength = 12;

    private static readonly char[] _phraseEnds = ['，', ',', '；', ';', '：', ':', '。', '（', '('];

    private static readonly string[] _frameworkAssemblies = ["Kmic.BuildingBlocks.Application", "Kmic.BuildingBlocks.Contracts"];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var applies = context.CompilationProvider.Select(static (compilation, _) => Applies(compilation.AssemblyName));
        var names = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
                static (syntax, ct) => Collect(syntax, ct))
            .SelectMany(static (entries, _) => entries)
            .Collect()
            .Combine(applies);

        context.RegisterSourceOutput(names, static (output, input) =>
        {
            if (input.Right)
            {
                Emit(output, input.Left);
            }
        });
    }

    private static bool Applies(string? assemblyName) =>
        KmicProject.ModuleLayer(assemblyName) is "Application" or "Contracts"
        || _frameworkAssemblies.Contains(assemblyName, StringComparer.Ordinal);

    private static ImmutableArray<(string Key, string Name)> Collect(GeneratorSyntaxContext syntax, CancellationToken ct)
    {
        if (syntax.SemanticModel.GetDeclaredSymbol(syntax.Node, ct) is not INamedTypeSymbol type)
        {
            return [];
        }

        var parameters = ParameterDocs(type.GetDocumentationCommentXml(cancellationToken: ct));
        var builder = ImmutableArray.CreateBuilder<(string, string)>();
        foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
        {
            if (property is not { DeclaredAccessibility: Accessibility.Public, IsStatic: false, IsIndexer: false })
            {
                continue;
            }

            var text = Summary(property.GetDocumentationCommentXml(cancellationToken: ct))
                ?? (parameters.TryGetValue(property.Name, out var parameter) ? parameter : null);
            if (ShortName(text) is { } name)
            {
                builder.Add(($"{MetadataName(type)}.{property.Name}", name));
            }
        }

        return builder.ToImmutable();
    }

    // 提取首个短语作为显示名；超长或不含中文时返回 null
    internal static string? ShortName(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var end = text!.IndexOfAny(_phraseEnds);
        var name = (end < 0 ? text : text[..end]).Trim();
        return name.Length is > 0 and <= MaxNameLength && name.Any(IsChinese) ? name : null;
    }

    private static bool IsChinese(char c) => c is >= '一' and <= '鿿';

    // 嵌套类型使用 + 连接，与 Type.FullName 保持一致
    private static string MetadataName(INamedTypeSymbol type)
    {
        var name = type.MetadataName;
        for (var outer = type.ContainingType; outer is not null; outer = outer.ContainingType)
        {
            name = $"{outer.MetadataName}+{name}";
        }

        return type.ContainingNamespace is { IsGlobalNamespace: false } ns ? $"{ns.ToDisplayString()}.{name}" : name;
    }

    private static string? Summary(string? xml) => Parse(xml)?.Element("summary") is { } summary ? Text(summary) : null;

    private static Dictionary<string, string> ParameterDocs(string? xml)
    {
        var docs = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var parameter in Parse(xml)?.Elements("param") ?? [])
        {
            if (parameter.Attribute("name")?.Value is { } name)
            {
                docs[name] = Text(parameter);
            }
        }

        return docs;
    }

    private static XElement? Parse(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return null;
        }

        try
        {
            return XElement.Parse(xml);
        }
        catch (System.Xml.XmlException)
        {
            // XML 格式错误由编译器直接告警，此处跳过
            return null;
        }
    }

    private static string Text(XElement element) =>
        string.Join(" ", element.Value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static void Emit(SourceProductionContext output, ImmutableArray<(string Key, string Name)> entries)
    {
        if (entries.IsDefaultOrEmpty)
        {
            return;
        }

        var source = new StringBuilder();
        source.AppendLine("// <auto-generated/>");
        source.AppendLine("#nullable enable");
        source.AppendLine("namespace Kmic.Generated;");
        source.AppendLine();
        source.AppendLine("[global::System.CodeDom.Compiler.GeneratedCode(\"Kmic.BuildingBlocks.Analyzers\", \"1.0.0\")]");
        source.AppendLine("internal static class DisplayNames");
        source.AppendLine("{");
        source.AppendLine("    public static global::System.Collections.Generic.IReadOnlyDictionary<string, string> Properties { get; } =");
        source.AppendLine("        new global::System.Collections.Generic.Dictionary<string, string>(global::System.StringComparer.Ordinal)");
        source.AppendLine("        {");

        // 分部类型会被多次收集，先去重并按键排序以保证生成结果稳定
        foreach (var (key, name) in entries.GroupBy(e => e.Key, StringComparer.Ordinal).Select(g => g.First()).OrderBy(e => e.Key, StringComparer.Ordinal))
        {
            source.Append("            [").Append(SymbolDisplay.FormatLiteral(key, quote: true)).Append("] = ")
                .Append(SymbolDisplay.FormatLiteral(name, quote: true)).AppendLine(",");
        }

        source.AppendLine("        };");
        source.AppendLine("}");
        output.AddSource("DisplayNames.g.cs", source.ToString());
    }
}
