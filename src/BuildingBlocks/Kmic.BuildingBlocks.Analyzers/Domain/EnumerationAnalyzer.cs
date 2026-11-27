using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.Domain;

/// <summary>KM0301：智能枚举必须声明为 sealed，构造函数私有，枚举值为 static readonly 字段。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnumerationAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor NotSealed = Rules.Create(
        "KM0301",
        Rules.DomainCategory,
        "智能枚举取值只能由类型自身声明",
        "智能枚举 {0} 必须声明为 sealed",
        "框架将类型内声明的静态字段视为全部枚举取值；子类或外部实例化的取值无法纳入注册表，按编码查找时将缺失。");

    internal static readonly DiagnosticDescriptor NonPrivateConstructor = Rules.Create(
        "KM0301",
        Rules.DomainCategory,
        "智能枚举取值只能由类型自身声明",
        "智能枚举 {0} 的构造函数必须为 private",
        "框架将类型内声明的静态字段视为全部枚举取值；子类或外部实例化的取值无法纳入注册表，按编码查找时将缺失。");

    internal static readonly DiagnosticDescriptor MutableValue = Rules.Create(
        "KM0301",
        Rules.DomainCategory,
        "智能枚举取值只能由类型自身声明",
        "智能枚举的取值 {0} 必须声明为 static readonly 字段",
        "框架将类型内声明的静态字段视为全部枚举取值；若字段非只读，运行时可能被篡改。");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [NotSealed, NonPrivateConstructor, MutableValue];

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
