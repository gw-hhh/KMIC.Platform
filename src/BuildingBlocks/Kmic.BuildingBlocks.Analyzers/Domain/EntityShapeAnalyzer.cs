using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.Domain;

/// <summary>KM0103～KM0105：规范实体的构造方式、属性只读性与集合封装。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EntityShapeAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor NonPrivateConstructor = Rules.Create(
        "KM0103",
        Rules.DomainCategory,
        "实体的构造函数必须私有",
        "实体 {0} 的构造函数必须为 private（抽象基类可为 protected），请通过工厂方法创建实例",
        "实体仅能通过工厂方法创建，以确保创建时的校验逻辑不被绕过。");

    internal static readonly DiagnosticDescriptor WritableProperty = Rules.Create(
        "KM0104",
        Rules.DomainCategory,
        "实体属性对外只读",
        "实体属性 {0} 仅允许 private set，请移除非私有的 setter 或 init",
        "实体的状态只能通过其业务方法变更。");

    internal static readonly DiagnosticDescriptor MutableCollection = Rules.Create(
        "KM0105",
        Rules.DomainCategory,
        "实体禁止公开可变集合",
        "实体成员 {0} 公开了可变集合，请改为 private readonly List<T> 字段并对外暴露 IReadOnlyList<T> 属性",
        "外部若直接持有可变集合，可能跳过实体的业务方法直接增删元素。");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [NonPrivateConstructor, WritableProperty, MutableCollection];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (!KmicProject.IsModuleDomain(start.Compilation.AssemblyName)
                || start.Compilation.GetTypeByMetadataName(KnownTypes.Entity) is not { } entity)
            {
                return;
            }

            start.RegisterSymbolAction(c => Analyze(c, entity), SymbolKind.NamedType);
        });
    }

    private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol entity)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (!KnownTypes.InheritsFrom(type, entity))
        {
            return;
        }

        foreach (var constructor in type.InstanceConstructors)
        {
            var allowed = constructor.DeclaredAccessibility == Accessibility.Private
                || (type.IsAbstract && constructor.DeclaredAccessibility is Accessibility.Protected or Accessibility.ProtectedAndInternal);
            if (!allowed)
            {
                // 未显式声明构造函数时，编译器会生成隐式 public 无参构造函数，此时将诊断定位到类型声明处
                var location = constructor.IsImplicitlyDeclared ? type.Locations[0] : constructor.Locations[0];
                context.ReportDiagnostic(Diagnostic.Create(NonPrivateConstructor, location, type.Name));
            }
        }

        foreach (var member in type.GetMembers())
        {
            if (member.IsImplicitlyDeclared)
            {
                continue;
            }

            if (member is IPropertySymbol { SetMethod: { } setter } writable
                && (setter.IsInitOnly || setter.DeclaredAccessibility != Accessibility.Private))
            {
                context.ReportDiagnostic(Diagnostic.Create(WritableProperty, writable.Locations[0], writable.Name));
            }

            var exposedType = member switch
            {
                IPropertySymbol property when property.DeclaredAccessibility != Accessibility.Private => property.Type,
                IFieldSymbol field when field.DeclaredAccessibility != Accessibility.Private => field.Type,
                _ => null,
            };
            if (exposedType is not null && Rules.IsMutableCollection(exposedType))
            {
                context.ReportDiagnostic(Diagnostic.Create(MutableCollection, member.Locations[0], member.Name));
            }
        }
    }
}
