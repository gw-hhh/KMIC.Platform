using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.Domain;

/// <summary>
/// KM0301：智能枚举要 sealed、构造函数私有、取值是 static readonly 字段
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnumerationAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor NotSealed = Rules.Create(
        "KM0301",
        Rules.DomainCategory,
        "智能枚举的取值只能由类型自身声明",
        "智能枚举 {0} 要声明为 sealed",
        "框架把类型里声明的静态字段当作全部取值。子类里的或在别处 new 出来的取值不在其中，按编码查不到。");

    internal static readonly DiagnosticDescriptor NonPrivateConstructor = Rules.Create(
        "KM0301",
        Rules.DomainCategory,
        "智能枚举的取值只能由类型自身声明",
        "智能枚举 {0} 的构造函数要改为 private",
        "框架把类型里声明的静态字段当作全部取值。子类里的或在别处 new 出来的取值不在其中，按编码查不到。");

    internal static readonly DiagnosticDescriptor MutableValue = Rules.Create(
        "KM0301",
        Rules.DomainCategory,
        "智能枚举的取值只能由类型自身声明",
        "智能枚举的取值 {0} 要声明为 static readonly 字段",
        "框架把类型里声明的静态字段当作全部取值，字段不是只读的话，运行时可能被换掉。");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [NotSealed, NonPrivateConstructor, MutableValue];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (!KmicProject.IsModuleDomain(start.Compilation.AssemblyName)
                || start.Compilation.GetTypeByMetadataName(KnownTypes.Enumeration) is not { } enumeration)
            {
                return;
            }

            start.RegisterSymbolAction(c => Analyze(c, enumeration), SymbolKind.NamedType);
        });
    }

    private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol enumeration)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (!KnownTypes.InheritsFrom(type, enumeration))
        {
            return;
        }

        if (!type.IsSealed)
        {
            context.ReportDiagnostic(Diagnostic.Create(NotSealed, type.Locations[0], type.Name));
        }

        foreach (var constructor in type.InstanceConstructors.Where(c => c.DeclaredAccessibility != Accessibility.Private))
        {
            var location = constructor.IsImplicitlyDeclared ? type.Locations[0] : constructor.Locations[0];
            context.ReportDiagnostic(Diagnostic.Create(NonPrivateConstructor, location, type.Name));
        }

        foreach (var field in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (SymbolEqualityComparer.Default.Equals(field.Type, type) && !field.IsImplicitlyDeclared && !(field.IsStatic && field.IsReadOnly))
            {
                context.ReportDiagnostic(Diagnostic.Create(MutableValue, field.Locations[0], field.Name));
            }
        }
    }
}
