using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Kmic.BuildingBlocks.Analyzers.Domain;

/// <summary>
/// KM0101：Error 只能在 *Errors 静态类里创建，Error.Unavailable 只能在 CommonErrors 里用
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorCatalogAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor OutsideCatalog = Rules.Create(
        "KM0101",
        Rules.DomainCategory,
        "错误只能在错误目录里创建",
        "Error.{0} 只能在命名为 *Errors 的静态类里调用，这里改为引用错误目录中的错误",
        "错误都在错误目录里，架构测试才能检查错误码全局唯一，前端也能按固定的错误码判断。");

    internal static readonly DiagnosticDescriptor UnavailableOutsideFramework = Rules.Create(
        "KM0101",
        Rules.DomainCategory,
        "错误只能在错误目录里创建",
        "Error.Unavailable 只给框架的 CommonErrors 用，业务错误目录里不能用",
        "Unavailable 只用于死锁、序列化失败这类临时故障，由框架转成可以重试的 503。");

    private static readonly ImmutableHashSet<string> _factories =
        ImmutableHashSet.Create(StringComparer.Ordinal, "Validation", "NotFound", "Conflict", "Forbidden", "BusinessRule", "Unavailable");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [OutsideCatalog, UnavailableOutsideFramework];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            // 测试要随手构造错误来验证 Result 和转换逻辑
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
