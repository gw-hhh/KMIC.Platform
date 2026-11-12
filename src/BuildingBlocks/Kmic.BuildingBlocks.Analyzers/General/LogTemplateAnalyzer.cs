using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.General;

/// <summary>
/// KM9008：日志模板不能用 {@对象} 把整个对象写进日志
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LogTemplateAnalyzer : DiagnosticAnalyzer
{
    private const string LoggerMessageAttribute = "Microsoft.Extensions.Logging.LoggerMessageAttribute";

    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM9008",
        Rules.GeneralCategory,
        "日志模板不能解构整个对象",
        "日志模板里的“{0}”会把整个对象写进日志，改成逐个写需要的属性",
        "整个对象里可能带着手机号、令牌这类敏感数据，日志里只写需要的、不敏感的属性。");

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
        if (attribute.ArgumentList is null
            || !string.Equals(context.SemanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol?.ContainingType?.ToDisplayString(), LoggerMessageAttribute, StringComparison.Ordinal))
        {
            return;
        }

        foreach (var argument in attribute.ArgumentList.Arguments)
        {
            if (context.SemanticModel.GetConstantValue(argument.Expression, context.CancellationToken).Value is not string text)
            {
                continue;
            }

            var start = text.IndexOf("{@", StringComparison.Ordinal);
            if (start >= 0)
            {
                var end = text.IndexOf('}', start);
                context.ReportDiagnostic(Diagnostic.Create(Rule, argument.Expression.GetLocation(), end > start ? text.Substring(start, end - start + 1) : "{@"));
            }
        }
    }
}
