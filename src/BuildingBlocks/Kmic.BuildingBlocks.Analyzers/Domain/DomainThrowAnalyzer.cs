using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Kmic.BuildingBlocks.Analyzers.Domain;

/// <summary>
/// KM0102：Domain 里只能抛参数异常、InvalidOperationException 和 DomainInvariantException
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DomainThrowAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM0102",
        Rules.DomainCategory,
        "领域层只能抛表示程序缺陷的异常",
        "领域层不能抛 {0}，业务失败改为返回 Result",
        "能抛的只有两类程序缺陷：调用方用错（ArgumentException 及其子类、InvalidOperationException）和不变量被破坏（DomainInvariantException）.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (!KmicProject.IsModuleDomain(start.Compilation.AssemblyName))
            {
                return;
            }

            ImmutableArray<INamedTypeSymbol?> allowed =
            [
                start.Compilation.GetTypeByMetadataName("System.ArgumentException"),
                start.Compilation.GetTypeByMetadataName("System.InvalidOperationException"),
                start.Compilation.GetTypeByMetadataName(KnownTypes.DomainInvariantException),
            ];
            start.RegisterOperationAction(c => Analyze(c, allowed), OperationKind.Throw);
        });
    }

    private static void Analyze(OperationAnalysisContext context, ImmutableArray<INamedTypeSymbol?> allowed)
    {
        // throw; 抛的是原异常，不算新的异常类型
        if (((IThrowOperation)context.Operation).Exception is not { } exception)
        {
            return;
        }

        var operand = exception;
        while (operand is IConversionOperation conversion)
        {
            operand = conversion.Operand;
        }

        if (operand.Type is not { } thrown || IsAllowed(thrown, allowed))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), thrown.Name));
    }

    private static bool IsAllowed(ITypeSymbol thrown, ImmutableArray<INamedTypeSymbol?> allowed)
    {
        for (var current = thrown; current is not null; current = current.BaseType)
        {
            if (allowed.Any(a => SymbolEqualityComparer.Default.Equals(a, current)))
            {
                return true;
            }
        }

        return false;
    }
}
