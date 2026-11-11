using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>
/// KM9003：TODO、FIXME 和测试的 Skip 必须带跟踪编号，不能用 HACK
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TrackedMarkerAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor UntrackedMarker = Rules.Create(
        "KM9003",
        Rules.GeneralCategory,
        "待办和跳过的测试必须带跟踪编号",
        "{0} 要带跟踪编号，写成 {0}(#123): 说明",
        "没有编号的待办没人跟进，会一直留在代码里.");

    internal static readonly DiagnosticDescriptor Hack = Rules.Create(
        "KM9003",
        Rules.GeneralCategory,
        "待办和跳过的测试必须带跟踪编号",
        "不用 HACK，改写成说明原因的普通注释，需要以后清理的加 TODO(#编号)",
        "HACK 只说明这里有问题，不说明原因，也没人跟进.");

    internal static readonly DiagnosticDescriptor UntrackedSkip = Rules.Create(
        "KM9003",
        Rules.GeneralCategory,
        "待办和跳过的测试必须带跟踪编号",
        "跳过的测试要在 Skip 里写明跟踪编号，如 Skip = \"#123 等待接口上线\"",
        "没有编号的跳过没人跟进，测试会一直不运行.");

    // 超时给 1 秒：并行编译时线程会被挤占，100 毫秒曾经误报超时。这几个模式不会回溯，正常用不到超时
    private static readonly TimeSpan _matchTimeout = TimeSpan.FromSeconds(1);
    private static readonly Regex _marker = new(@"\b(?<marker>TODO|FIXME)\b(?!\(#\d+\))", RegexOptions.CultureInvariant, _matchTimeout);
    private static readonly Regex _hack = new(@"\bHACK\b", RegexOptions.CultureInvariant, _matchTimeout);
    private static readonly Regex _ticket = new(@"#\d+", RegexOptions.CultureInvariant, _matchTimeout);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [UntrackedMarker, Hack, UntrackedSkip];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeComments);
        context.RegisterSyntaxNodeAction(AnalyzeSkip, SyntaxKind.AttributeArgument);
    }

    private static void AnalyzeComments(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            if (!IsComment(trivia))
            {
                continue;
            }

            var text = trivia.ToString();
            foreach (Match match in _marker.Matches(text))
            {
                var location = Location.Create(context.Tree, new Microsoft.CodeAnalysis.Text.TextSpan(trivia.SpanStart + match.Index, match.Length));
                context.ReportDiagnostic(Diagnostic.Create(UntrackedMarker, location, match.Groups["marker"].Value));
            }

            var hack = _hack.Match(text);
            if (hack.Success)
            {
                var location = Location.Create(context.Tree, new Microsoft.CodeAnalysis.Text.TextSpan(trivia.SpanStart + hack.Index, hack.Length));
                context.ReportDiagnostic(Diagnostic.Create(Hack, location));
            }
        }
    }

    private static void AnalyzeSkip(SyntaxNodeAnalysisContext context)
    {
        var argument = (AttributeArgumentSyntax)context.Node;
        if (!string.Equals(argument.NameEquals?.Name.Identifier.ValueText, "Skip", StringComparison.Ordinal))
        {
            return;
        }

        if (context.SemanticModel.GetConstantValue(argument.Expression, context.CancellationToken).Value is not string reason
            || !_ticket.IsMatch(reason))
        {
            context.ReportDiagnostic(Diagnostic.Create(UntrackedSkip, argument.GetLocation()));
        }
    }

    private static bool IsComment(SyntaxTrivia trivia) =>
        trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
        || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
        || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
        || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
}
