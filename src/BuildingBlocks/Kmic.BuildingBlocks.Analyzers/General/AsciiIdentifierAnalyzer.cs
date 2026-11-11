using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>
/// KM9004：标识符只能用 ASCII 字符
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsciiIdentifierAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM9004",
        Rules.GeneralCategory,
        "标识符只能用 ASCII 字符",
        "标识符 {0} 含有非 ASCII 字符，改用术语表中的英文名",
        "同一个概念只用一个英文名，代码、测试、术语表保持一致.");

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
        foreach (var token in root.DescendantTokens())
        {
            // 只报声明处，引用处指向的是同一个声明，不重复报
            if (token.IsKind(SyntaxKind.IdentifierToken)
                && token.Parent is not IdentifierNameSyntax
                && token.ValueText.Any(c => c > 127))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, token.GetLocation(), token.ValueText));
            }
        }
    }
}
