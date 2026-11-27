using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>KM9006：禁止使用 dynamic。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DynamicAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM9006",
        Rules.GeneralCategory,
        "禁止使用 dynamic",
        "禁止使用 dynamic，请改用明确类型或 JsonElement",
        "使用 dynamic 会绕过编译期类型检查，错误将延迟至运行时暴露。");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

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
