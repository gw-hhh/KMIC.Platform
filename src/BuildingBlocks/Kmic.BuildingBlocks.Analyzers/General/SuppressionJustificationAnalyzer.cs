using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>
/// KM9001：[SuppressMessage] 必须写明理由
/// </summary>
/// <remarks>
/// 每个豁免都是要评审的决定，写了理由才能被找到和复查
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SuppressionJustificationAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM9001",
        Rules.GeneralCategory,
        "豁免必须写明理由",
        "[{0}] 必须用 Justification 写明豁免的理由");

    private static readonly ImmutableHashSet<string> _suppressionAttributes = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "System.Diagnostics.CodeAnalysis.SuppressMessageAttribute",
        "System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessageAttribute");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.Attribute);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var attribute = (AttributeSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol is not IMethodSymbol { ContainingType: var type }
            || !_suppressionAttributes.Contains(type.ToDisplayString()))
        {
            return;
        }

        var justification = attribute.ArgumentList?.Arguments
            .FirstOrDefault(a => string.Equals(a.NameEquals?.Name.Identifier.ValueText, "Justification", StringComparison.Ordinal));
        var text = justification is null
            ? null
            : context.SemanticModel.GetConstantValue(justification.Expression, context.CancellationToken).Value as string;
        if (string.IsNullOrWhiteSpace(text))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, attribute.GetLocation(), type.Name.Replace("Attribute", string.Empty)));
        }
    }
}
