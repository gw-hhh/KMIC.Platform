using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>
/// KM9003：TODO、FIXME 与测试的 Skip 必须带跟踪编号；不使用 HACK
/// </summary>
/// <remarks>
/// 没有编号的待办没人跟进，会永远留在代码里（06 第 10.3、12.3 节）
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TrackedMarkerAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor UntrackedMarker = Rules.Create(
        "KM9003",
        Rules.GeneralCategory,
        "待办与跳过必须带跟踪编号",
        "{0} 必须带跟踪编号，写成 {0}(#123): 说明");

    internal static readonly DiagnosticDescriptor Hack = Rules.Create(
        "KM9003",
        Rules.GeneralCategory,
        "待办与跳过必须带跟踪编号",
        "不使用 HACK：改写成说明原因的普通注释，需要以后清理的加 TODO(#编号)");

    internal static readonly DiagnosticDescriptor UntrackedSkip = Rules.Create(
        "KM9003",
        Rules.GeneralCategory,
        "待办与跳过必须带跟踪编号",
        "跳过测试必须在 Skip 中写明跟踪编号，如 Skip = \"#123 等待接口上线\"");

    private static readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(100);
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
