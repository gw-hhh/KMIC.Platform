using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>KM9005：禁止对主构造函数参数重新赋值。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PrimaryConstructorParameterAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM9005",
        Rules.GeneralCategory,
        "禁止对主构造函数参数重新赋值",
        "禁止对主构造函数参数 {0} 重新赋值，需要变更的值请另行声明字段或局部变量",
        "主构造函数参数在整个类型作用域内可见，重新赋值会导致阅读代码时难以推断其当前值。");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(
            Analyze,
            OperationKind.SimpleAssignment,
            OperationKind.CompoundAssignment,
            OperationKind.CoalesceAssignment,
            OperationKind.Increment,
            OperationKind.Decrement);
    }

    private static void Analyze(OperationAnalysisContext context)
    {
        var target = context.Operation switch
        {
            IAssignmentOperation assignment => assignment.Target,
            IIncrementOrDecrementOperation increment => increment.Target,
            _ => null,
        };

        if (target is IParameterReferenceOperation { Parameter: var parameter }
            && parameter.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
            && constructor.DeclaringSyntaxReferences.Any(r => r.GetSyntax(context.CancellationToken) is TypeDeclarationSyntax))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, target.Syntax.GetLocation(), parameter.Name));
        }
    }
}
