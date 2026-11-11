using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>
/// KM9007：不能嵌套三元运算，加括号也不行
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NestedConditionalAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM9007",
        Rules.GeneralCategory,
        "不能嵌套三元运算",
        "三元运算不能嵌套，多个条件用 switch 表达式，涉及顺序或副作用时用 if / else",
        "嵌套的三元运算要逐层拆开才看得懂，加了括号也一样.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ConditionalExpression);
    }

    // 只看到最近的函数边界：lambda 里的三元和外面的三元不算嵌套
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        for (var parent = context.Node.Parent; parent is not null; parent = parent.Parent)
        {
            if (parent is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax or MemberDeclarationSyntax)
            {
                return;
            }

            if (parent is ConditionalExpressionSyntax)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
                return;
            }
        }
    }
}
