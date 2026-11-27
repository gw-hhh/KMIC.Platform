using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.Domain;

/// <summary>KM0201～KM0205：领域层中的 record 均按值对象规范检查（领域事件除外）。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ValueObjectAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Shape = Rules.Create(
        "KM0201",
        Rules.DomainCategory,
        "值对象必须为非位置参数的 sealed record",
        "值对象 {0} 必须声明为 sealed record class，且不能使用位置参数",
        "位置参数会生成公开构造函数及公有 init 属性，导致外部可绕过工厂方法直接实例化或通过 with 表达式篡改。");

    internal static readonly DiagnosticDescriptor NonPrivateConstructor = Rules.Create(
        "KM0202",
        Rules.DomainCategory,
        "值对象的构造函数必须私有",
        "值对象 {0} 的构造函数必须为 private，请通过返回 Result 的工厂方法创建",
        "值对象仅能通过工厂方法创建，以确保校验逻辑不被绕过。");

    internal static readonly DiagnosticDescriptor PropertyAccessor = Rules.Create(
        "KM0203",
        Rules.DomainCategory,
        "值对象属性必须是 { get; private init; }",
        "值对象属性 {0} 必须声明为 {{ get; private init; }}",
        "仅有 getter 的属性不会被 EF 映射且遗漏时不报错；private init 同时可防止外部通过 with 表达式修改。");

    internal static readonly DiagnosticDescriptor MissingFactory = Rules.Create(
        "KM0204",
        Rules.DomainCategory,
        "值对象必须有返回 Result 的工厂方法",
        "值对象 {0} 至少须提供一个返回 Result<{0}> 的静态工厂方法",
        "因构造函数为私有，实例须通过工厂方法创建，并在校验失败时返回 Error 而非抛出异常。");

    internal static readonly DiagnosticDescriptor CollectionProperty = Rules.Create(
        "KM0205",
        Rules.DomainCategory,
        "值对象的集合属性必须只读且自定义相等比较",
        "值对象集合属性 {0} 必须使用 IReadOnlyList<T> 类型，且值对象须自行实现 Equals",
        "record 默认按引用比较集合成员，会导致元素内容相同的两个值对象被判为不相等。");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [Shape, NonPrivateConstructor, PropertyAccessor, MissingFactory, CollectionProperty];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (!KmicProject.IsModuleDomain(start.Compilation.AssemblyName))
            {
                return;
            }

            var domainEvent = start.Compilation.GetTypeByMetadataName(KnownTypes.DomainEvent);
            var resultOfT = start.Compilation.GetTypeByMetadataName(KnownTypes.ResultOfT);
            var readOnlyList = start.Compilation.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyList_T);
            start.RegisterSymbolAction(c => Analyze(c, domainEvent, resultOfT, readOnlyList), SymbolKind.NamedType);
        });
    }

    private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol? domainEvent, INamedTypeSymbol? resultOfT, INamedTypeSymbol readOnlyList)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (!type.IsRecord || KnownTypes.Implements(type, domainEvent))
        {
            return;
        }

        var location = type.Locations[0];
        var positional = type.DeclaringSyntaxReferences
            .Any(r => r.GetSyntax(context.CancellationToken) is RecordDeclarationSyntax { ParameterList: not null });
        if (!type.IsSealed || type.IsValueType || positional)
        {
            context.ReportDiagnostic(Diagnostic.Create(Shape, location, type.Name));
        }

        if (type.InstanceConstructors.Any(c => !c.IsImplicitlyDeclared && c.DeclaredAccessibility != Accessibility.Private))
        {
            context.ReportDiagnostic(Diagnostic.Create(NonPrivateConstructor, location, type.Name));
        }

        if (!HasFactory(type, resultOfT))
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFactory, location, type.Name));
        }

        var definesEquality = type.GetMembers("Equals").OfType<IMethodSymbol>()
            .Any(m => !m.IsImplicitlyDeclared && m.Parameters is [{ Type: var parameter }] && SymbolEqualityComparer.Default.Equals(parameter, type));
        foreach (var property in type.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsImplicitlyDeclared))
        {
            if (IsAutoProperty(property, context.CancellationToken)
                && property.SetMethod is not { IsInitOnly: true, DeclaredAccessibility: Accessibility.Private })
            {
                context.ReportDiagnostic(Diagnostic.Create(PropertyAccessor, property.Locations[0], property.Name));
            }

            if (Rules.IsCollection(property.Type)
                && (!SymbolEqualityComparer.Default.Equals(property.Type.OriginalDefinition, readOnlyList) || !definesEquality))
            {
                context.ReportDiagnostic(Diagnostic.Create(CollectionProperty, property.Locations[0], property.Name));
            }
        }
    }

    private static bool HasFactory(INamedTypeSymbol type, INamedTypeSymbol? resultOfT) =>
        resultOfT is not null
        && type.GetMembers().OfType<IMethodSymbol>().Any(m =>
            m is { IsStatic: true, MethodKind: MethodKind.Ordinary }
            && m.DeclaredAccessibility != Accessibility.Private
            && m.ReturnType is INamedTypeSymbol { TypeArguments: [var value] } returned
            && SymbolEqualityComparer.Default.Equals(returned.OriginalDefinition, resultOfT)
            && SymbolEqualityComparer.Default.Equals(value, type));

    // 计算属性无需持久化映射，不受 KM0203 约束
    private static bool IsAutoProperty(IPropertySymbol property, CancellationToken ct) =>
        property.DeclaringSyntaxReferences.All(reference =>
            reference.GetSyntax(ct) is PropertyDeclarationSyntax { ExpressionBody: null, AccessorList: { } accessors }
            && accessors.Accessors.All(a => a is { Body: null, ExpressionBody: null }));
}
