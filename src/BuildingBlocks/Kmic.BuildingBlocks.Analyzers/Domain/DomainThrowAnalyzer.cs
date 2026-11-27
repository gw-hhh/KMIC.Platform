using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Kmic.BuildingBlocks.Analyzers.Domain;

/// <summary>KM0102：领域层仅允许抛出参数异常、InvalidOperationException 和 DomainInvariantException。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DomainThrowAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM0102",
        Rules.DomainCategory,
        "领域层禁止抛出业务异常",
        "领域层禁止抛出 {0}，业务失败请改为返回 Result",
        "业务失败（如状态不符、余额不足）应返回 Result。仅在程序缺陷时抛出异常：参数错误用 ArgumentException，调用时机不符用 InvalidOperationException，领域不变量被破坏用 DomainInvariantException。");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

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
        // throw; 重新抛出原异常，不视为新增的异常类型
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
