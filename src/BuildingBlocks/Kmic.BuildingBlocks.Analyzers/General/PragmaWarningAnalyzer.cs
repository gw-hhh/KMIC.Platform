using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>
/// KM9002：不能用 #pragma warning disable
/// </summary>
/// <remarks>
/// 忘了 restore 会悄悄扩大豁免范围，也不留理由。改用带理由的 [SuppressMessage]
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PragmaWarningAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM9002",
        Rules.GeneralCategory,
        "不得使用 #pragma warning disable",
        "不得使用 #pragma warning disable，改用带理由的 [SuppressMessage]，作用于最小范围");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(Analyze);
    }

    private static void Analyze(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.GetStructure() is PragmaWarningDirectiveTriviaSyntax pragma
                && pragma.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, pragma.GetLocation()));
            }
        }
    }
}
