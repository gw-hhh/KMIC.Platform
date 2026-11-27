using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>KM9007：禁止嵌套三元条件运算符。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NestedConditionalAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM9007",
        Rules.GeneralCategory,
        "禁止嵌套三元条件运算符",
        "禁止嵌套三元条件运算符；多分支请改用 switch 表达式，涉及执行顺序或副作用时请使用 if / else",
        "嵌套的三元条件运算符可读性差，即使添加括号也难以直观理解。");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ConditionalExpression);
    }

    // 仅向上检查至最近的函数边界：Lambda 内部的三元运算与外层不构成嵌套
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
