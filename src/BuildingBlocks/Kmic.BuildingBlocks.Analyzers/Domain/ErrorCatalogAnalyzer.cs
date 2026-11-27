using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Kmic.BuildingBlocks.Analyzers.Domain;

/// <summary>KM0101：Error 只能在 *Errors 静态类中创建，Error.Unavailable 仅限 CommonErrors 使用。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorCatalogAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor OutsideCatalog = Rules.Create(
        "KM0101",
        Rules.DomainCategory,
        "错误只能在错误目录中创建",
        "Error.{0} 只能在命名为 *Errors 的静态类中调用，此处请改为引用错误目录中定义的错误",
        "将错误集中定义在错误目录中，便于架构测试校验错误码全局唯一性，前端也能依据固定错误码进行处理。");

    internal static readonly DiagnosticDescriptor UnavailableOutsideFramework = Rules.Create(
        "KM0101",
        Rules.DomainCategory,
        "错误只能在错误目录中创建",
        "Error.Unavailable 仅限框架 CommonErrors 使用，业务错误目录禁止使用",
        "Unavailable 仅用于死锁、序列化失败等瞬时故障，由框架映射为可重试的 HTTP 503。");

    private static readonly ImmutableHashSet<string> _factories =
        ImmutableHashSet.Create(StringComparer.Ordinal, "Validation", "NotFound", "Conflict", "Forbidden", "BusinessRule", "Unavailable");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [OutsideCatalog, UnavailableOutsideFramework];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            // 测试项目中允许直接构造 Error，以验证 Result 及转换逻辑
            if (KmicProject.IsTestProject(start.Compilation.AssemblyName)
                || start.Compilation.GetTypeByMetadataName(KnownTypes.Error) is not { } error)
            {
                return;
            }

            var commonErrors = start.Compilation.GetTypeByMetadataName(KnownTypes.CommonErrors);
            start.RegisterOperationAction(c => Analyze(c, error, commonErrors), OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context, INamedTypeSymbol error, INamedTypeSymbol? commonErrors)
    {
        var method = ((IInvocationOperation)context.Operation).TargetMethod;
        if (!method.IsStatic || !_factories.Contains(method.Name) || !SymbolEqualityComparer.Default.Equals(method.ContainingType, error))
        {
            return;
        }

        var caller = context.ContainingSymbol.ContainingType;
        var location = context.Operation.Syntax.GetLocation();
        if (string.Equals(method.Name, "Unavailable", StringComparison.Ordinal))
        {
            if (!SymbolEqualityComparer.Default.Equals(caller, commonErrors))
            {
                context.ReportDiagnostic(Diagnostic.Create(UnavailableOutsideFramework, location));
            }

            return;
        }

        if (caller is not { IsStatic: true } || !caller.Name.EndsWith("Errors", StringComparison.Ordinal))
        {
            context.ReportDiagnostic(Diagnostic.Create(OutsideCatalog, location, method.Name));
        }
    }
}
