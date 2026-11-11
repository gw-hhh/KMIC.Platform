using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.Data;

/// <summary>
/// KM2001：查询里不能用 ToLower、ToUpper、Contains、StartsWith、EndsWith 匹配文本
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TextMatchingAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM2001",
        Rules.DataCategory,
        "查询里不能用字符串方法匹配文本",
        "查询里不能用 string.{0} 匹配文本，模糊匹配改用 KmicFunctions.ContainsText 或 StartsWithText，忽略大小写的等值比较把列声明为 IsCaseInsensitive()",
        "这些写法要么用不上索引，要么在不区分大小写的列上直接报错。只检查会翻译成 SQL 的查询，内存里的 LINQ 不受影响.");

    private static readonly ImmutableHashSet<string> _methods = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "ToLower",
        "ToLowerInvariant",
        "ToUpper",
        "ToUpperInvariant",
        "Contains",
        "StartsWith",
        "EndsWith");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            var layer = KmicProject.ModuleLayer(start.Compilation.AssemblyName);
            if (layer is not ("Application" or "Infrastructure"))
            {
                return;
            }

            var expression = start.Compilation.GetTypeByMetadataName("System.Linq.Expressions.Expression`1");
            var queryable = start.Compilation.GetTypeByMetadataName("System.Linq.IQueryable");
            start.RegisterSyntaxNodeAction(c => Analyze(c, expression, queryable), SyntaxKind.InvocationExpression);
        });
    }

    private static void Analyze(SyntaxNodeAnalysisContext context, INamedTypeSymbol? expression, INamedTypeSymbol? queryable)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax member
            || !_methods.Contains(member.Name.Identifier.ValueText)
            || context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol method
            || method.IsStatic
            || method.ContainingType.SpecialType != SpecialType.System_String
            || !InQuery(invocation, context.SemanticModel, expression, queryable, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, member.Name.GetLocation(), method.Name));
    }

    private static bool InQuery(
        SyntaxNode node,
        SemanticModel model,
        INamedTypeSymbol? expression,
        INamedTypeSymbol? queryable,
        CancellationToken ct)
    {
        foreach (var ancestor in node.Ancestors())
        {
            switch (ancestor)
            {
                case LambdaExpressionSyntax lambda
                    when model.GetTypeInfo(lambda, ct).ConvertedType is INamedTypeSymbol converted
                        && SymbolEqualityComparer.Default.Equals(converted.OriginalDefinition, expression):
                    return true;
                case QueryExpressionSyntax query
                    when model.GetTypeInfo(query.FromClause.Expression, ct).Type is { } source
                        && (SymbolEqualityComparer.Default.Equals(source, queryable) || KnownTypes.Implements(source, queryable)):
                    return true;
                case MemberDeclarationSyntax:
                    return false;
            }
        }

        return false;
    }
}
