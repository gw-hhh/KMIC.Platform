using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.Domain;

/// <summary>
/// KM0501：领域事件不能带实体对象
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DomainEventAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Rule = Rules.Create(
        "KM0501",
        Rules.DomainCategory,
        "领域事件不能带实体对象",
        "领域事件 {0} 的属性 {1} 带了实体 {2}，改为带它的 ID",
        "事件处理器拿到实体就能直接改它，绕过它的业务方法.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (!KmicProject.IsModuleDomain(start.Compilation.AssemblyName)
                || start.Compilation.GetTypeByMetadataName(KnownTypes.DomainEvent) is not { } domainEvent
                || start.Compilation.GetTypeByMetadataName(KnownTypes.Entity) is not { } entity)
            {
                return;
            }

            start.RegisterSymbolAction(c => Analyze(c, domainEvent, entity), SymbolKind.NamedType);
        });
    }

    private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol domainEvent, INamedTypeSymbol entity)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (!KnownTypes.Implements(type, domainEvent))
        {
            return;
        }

        foreach (var property in type.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsImplicitlyDeclared || p.Locations.Any(l => l.IsInSource)))
        {
            if (FindEntity(property.Type, entity) is { } carried)
            {
                var location = property.Locations.FirstOrDefault(l => l.IsInSource) ?? type.Locations[0];
                context.ReportDiagnostic(Diagnostic.Create(Rule, location, type.Name, property.Name, carried.Name));
            }
        }
    }

    // 集合、可空、元组里的实体同样不允许
    private static ITypeSymbol? FindEntity(ITypeSymbol type, INamedTypeSymbol entity)
    {
        if (KnownTypes.InheritsFrom(type, entity))
        {
            return type;
        }

        return type switch
        {
            IArrayTypeSymbol array => FindEntity(array.ElementType, entity),
            INamedTypeSymbol { IsGenericType: true } generic => generic.TypeArguments.Select(t => FindEntity(t, entity)).FirstOrDefault(t => t is not null),
            _ => null,
        };
    }
}
