using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>KM9004：标识符仅允许使用 ASCII 字符。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsciiIdentifierAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM9004",
        Rules.GeneralCategory,
        "标识符仅限使用 ASCII 字符",
        "标识符 {0} 包含非 ASCII 字符，请改用术语表中的英文命名",
        "同一概念应采用唯一英文命名，保证代码、测试与术语表一致。");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

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
            // 仅在声明处报告诊断，引用处指向同一声明，避免重复告警
            if (token.IsKind(SyntaxKind.IdentifierToken)
                && token.Parent is not IdentifierNameSyntax
                && token.ValueText.Any(c => c > 127))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, token.GetLocation(), token.ValueText));
            }
        }
    }
}
