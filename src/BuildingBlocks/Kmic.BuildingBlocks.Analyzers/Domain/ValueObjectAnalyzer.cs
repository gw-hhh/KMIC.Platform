using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kmic.BuildingBlocks.Analyzers.Domain;

/// <summary>
/// KM0201～KM0205：Domain 里的 record 都按值对象检查，领域事件除外
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ValueObjectAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor Shape = Rules.Create(
        "KM0201",
        Rules.DomainCategory,
        "值对象必须是 sealed record，不用位置参数",
        "值对象 {0} 要改为 sealed record class，不用位置参数写法",
        "位置参数会生成公开的构造函数和 init 属性，类型外就能绕过工厂方法创建，或用 with 修改.");

    internal static readonly DiagnosticDescriptor NonPrivateConstructor = Rules.Create(
        "KM0202",
        Rules.DomainCategory,
        "值对象的构造函数必须私有",
        "值对象 {0} 的构造函数要改为 private，通过返回 Result 的工厂方法创建",
        "值对象只能通过工厂方法创建，校验才不会被绕过.");

    internal static readonly DiagnosticDescriptor PropertyAccessor = Rules.Create(
        "KM0203",
        Rules.DomainCategory,
        "值对象属性必须是 { get; private init; }",
        "值对象属性 {0} 要写成 {{ get; private init; }}",
        "只有 getter 的属性 EF 不会映射，漏了也不报错。private init 还能挡住类型外的 with 表达式.");

    internal static readonly DiagnosticDescriptor MissingFactory = Rules.Create(
        "KM0204",
        Rules.DomainCategory,
        "值对象必须有返回 Result 的工厂方法",
        "值对象 {0} 至少要有一个返回 Result<{0}> 的静态工厂方法",
        "构造函数是私有的，只能通过工厂方法创建，校验失败时返回错误而不是抛异常.");

    internal static readonly DiagnosticDescriptor CollectionProperty = Rules.Create(
        "KM0205",
        Rules.DomainCategory,
        "值对象的集合属性必须只读，并自己实现相等比较",
        "值对象属性 {0} 是集合，类型要改为 IReadOnlyList<T>，值对象要自己实现 Equals",
        "record 默认按引用比较集合，两个内容相同的值对象会被判为不相等.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [Shape, NonPrivateConstructor, PropertyAccessor, MissingFactory, CollectionProperty];

    /// <inheritdoc />
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

    // 计算属性本来就不映射，不受 KM0203 约束
    private static bool IsAutoProperty(IPropertySymbol property, CancellationToken ct) =>
        property.DeclaringSyntaxReferences.All(reference =>
            reference.GetSyntax(ct) is PropertyDeclarationSyntax { ExpressionBody: null, AccessorList: { } accessors }
            && accessors.Accessors.All(a => a is { Body: null, ExpressionBody: null }));
}
