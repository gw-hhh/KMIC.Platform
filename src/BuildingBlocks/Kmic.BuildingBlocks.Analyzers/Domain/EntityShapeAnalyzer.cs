using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.Domain;

/// <summary>
/// KM0103～KM0105：实体通过工厂方法创建，属性对外只读，不公开可变集合
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EntityShapeAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor NonPrivateConstructor = Rules.Create(
        "KM0103",
        Rules.DomainCategory,
        "实体的构造函数必须私有",
        "实体 {0} 的构造函数要改为 private（抽象基类可以是 protected），通过工厂方法创建",
        "实体只能通过工厂方法创建，创建时的校验才不会被绕过.");

    internal static readonly DiagnosticDescriptor WritableProperty = Rules.Create(
        "KM0104",
        Rules.DomainCategory,
        "实体属性对外只读",
        "实体属性 {0} 只能有 private set，去掉非私有的 setter 或 init",
        "实体的状态只能通过它的业务方法修改.");

    internal static readonly DiagnosticDescriptor MutableCollection = Rules.Create(
        "KM0105",
        Rules.DomainCategory,
        "实体不能公开可变集合",
        "实体成员 {0} 公开了可变集合，改为 private readonly List<T> 字段加 IReadOnlyList<T> 属性",
        "外部拿到可变集合就能直接增删元素，绕过实体的业务方法.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [NonPrivateConstructor, WritableProperty, MutableCollection];

    /// <inheritdoc />
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
                // 没写构造函数时编译器会补一个公开的无参构造函数，报在类型上
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
