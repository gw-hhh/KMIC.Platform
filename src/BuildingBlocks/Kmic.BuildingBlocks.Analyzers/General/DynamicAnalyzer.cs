using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>
/// KM9006：不能用 dynamic
/// </summary>
/// <remarks>
/// dynamic 让编译器的类型检查失效，错误要到运行时才暴露
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DynamicAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM9006",
        Rules.GeneralCategory,
        "不得使用 dynamic",
        "不得使用 dynamic，改用明确的类型或 JsonElement");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.IdentifierName);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var name = (IdentifierNameSyntax)context.Node;
        if (string.Equals(name.Identifier.ValueText, "dynamic", StringComparison.Ordinal)
            && context.SemanticModel.GetTypeInfo(name, context.CancellationToken).Type?.TypeKind == TypeKind.Dynamic)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, name.GetLocation()));
        }
    }
}
