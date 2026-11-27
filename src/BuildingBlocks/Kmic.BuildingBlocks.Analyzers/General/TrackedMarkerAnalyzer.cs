using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>KM9003：TODO、FIXME 及测试 Skip 必须附带跟踪编号，禁止使用 HACK。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TrackedMarkerAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor UntrackedMarker = Rules.Create(
        "KM9003",
        Rules.GeneralCategory,
        "待办标记与跳过测试必须附带跟踪编号",
        "{0} 必须附带跟踪编号，格式为 {0}(#123): 说明",
        "缺少跟踪编号的待办事项难以追踪跟进，易长期滞留于代码中。");

    internal static readonly DiagnosticDescriptor Hack = Rules.Create(
        "KM9003",
        Rules.GeneralCategory,
        "待办标记与跳过测试必须附带跟踪编号",
        "禁止使用 HACK，请改写为说明原因的普通注释；若需后续清理，请使用 TODO(#编号)",
        "HACK 仅标示存在问题而不说明缘由，且缺乏后续跟踪机制。");

    internal static readonly DiagnosticDescriptor UntrackedSkip = Rules.Create(
        "KM9003",
        Rules.GeneralCategory,
        "待办标记与跳过测试必须附带跟踪编号",
        "跳过的测试必须在 Skip 中注明跟踪编号，例如 Skip = \"#123 等待接口上线\"",
        "缺少跟踪编号的跳过测试容易被遗忘，导致长期处于未执行状态。");

    // 正则匹配超时设为 1 秒，避免并行编译线程争抢时 100 毫秒误报超时；当前正则无回溯，正常情况下不会触发超时
    private static readonly TimeSpan _matchTimeout = TimeSpan.FromSeconds(1);
    private static readonly Regex _marker = new(@"\b(?<marker>TODO|FIXME)\b(?!\(#\d+\))", RegexOptions.CultureInvariant, _matchTimeout);
    private static readonly Regex _hack = new(@"\bHACK\b", RegexOptions.CultureInvariant, _matchTimeout);
    private static readonly Regex _ticket = new(@"#\d+", RegexOptions.CultureInvariant, _matchTimeout);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [UntrackedMarker, Hack, UntrackedSkip];

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
