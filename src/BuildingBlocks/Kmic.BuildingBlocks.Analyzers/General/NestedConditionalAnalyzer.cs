using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>
/// KM9007：手写代码禁止嵌套三元运算，加括号也不行
/// </summary>
/// <remarks>
/// 多条件的结果用 switch 表达式；涉及顺序或副作用用平铺的 if / else（06 附录 A.7）
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NestedConditionalAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM9007",
        Rules.GeneralCategory,
        "禁止嵌套三元运算",
        "禁止嵌套三元运算：多条件的结果用 switch 表达式，涉及顺序或副作用用 if / else");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ConditionalExpression);
    }

    // 只看到最近的函数边界为止：lambda 里的三元与外面的三元不算嵌套
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
