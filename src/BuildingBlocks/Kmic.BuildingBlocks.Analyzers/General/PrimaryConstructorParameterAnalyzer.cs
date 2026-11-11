using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>
/// KM9005：主构造函数的参数不能重新赋值
/// </summary>
/// <remarks>
/// 主构造函数参数在整个类型里都能用，被改过之后，读代码的人就没法确定它在某处的值
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PrimaryConstructorParameterAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM9005",
        Rules.GeneralCategory,
        "主构造函数参数不得重新赋值",
        "主构造函数参数 {0} 不得重新赋值，需要变化的值另建字段或局部变量");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
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
