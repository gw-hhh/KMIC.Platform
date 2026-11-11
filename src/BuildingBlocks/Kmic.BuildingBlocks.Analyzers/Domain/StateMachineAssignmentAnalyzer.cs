using System.Collections.Concurrent;
using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Kmic.BuildingBlocks.Analyzers.Domain;

/// <summary>
/// KM0401：状态属性只能由状态机修改
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StateMachineAssignmentAnalyzer : DiagnosticAnalyzer
{
    internal const string DiagnosticId = "KM0401";

    private const string Title = "状态属性只能由状态机改写";
    private const string Reason = "直接赋值会跳过状态机对状态变化的检查，也不会记下这次变化。";
    private const string DefineMethodName = "Define";

    internal static readonly DiagnosticDescriptor DirectAssignment = Rules.Create(
        DiagnosticId,
        Rules.DomainCategory,
        Title,
        "状态属性 {0} 不能直接赋值，改为调用状态机的 Start 或 Fire",
        Reason);

    // 要等整个编译结束才能和 Define 对上，IDE 默认不实时显示，构建时一定会报
    internal static readonly DiagnosticDescriptor AssignmentOutsideAggregate = Rules.Create(
        DiagnosticId,
        Rules.DomainCategory,
        Title,
        "状态属性 {0} 不能在其他类型里赋值，改为调用状态机的 Start 或 Fire",
        Reason,
        WellKnownDiagnosticTags.CompilationEnd);

    internal static readonly DiagnosticDescriptor SetterNotPrivate = Rules.Create(
        DiagnosticId,
        Rules.DomainCategory,
        Title,
        "状态属性 {0} 的 setter 要改为 private",
        Reason);

    internal static readonly DiagnosticDescriptor NotAutoProperty = Rules.Create(
        DiagnosticId,
        Rules.DomainCategory,
        Title,
        "状态属性 {0} 要写成自动属性",
        "有自己的字段时，类型里的代码可以绕过属性直接改字段，检查就拦不住。");

    internal static readonly DiagnosticDescriptor InvalidGetter = Rules.Create(
        DiagnosticId,
        Rules.DomainCategory,
        Title,
        "Define 的第一个参数只能直接返回聚合根的一个属性，如 s => s.Status",
        "分析器按这个参数认出哪个属性是状态，写成别的表达式就认不出来。");

    internal static readonly DiagnosticDescriptor InvalidSetter = Rules.Create(
        DiagnosticId,
        Rules.DomainCategory,
        Title,
        "Define 的第二个参数只能给第一个参数里的属性赋值，如 (s, v) => s.Status = v",
        "状态机通过这个参数写入状态，在里面改别的属性，就绕过了业务方法。");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [DirectAssignment, AssignmentOutsideAggregate, SetterNotPrivate, NotAutoProperty, InvalidGetter, InvalidSetter];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.GetTypeByMetadataName(KnownTypes.StateMachine) is not { } stateMachine)
            {
                return;
            }

            var allRegistrations = new ConcurrentBag<Registration>();
            var outsideAssignments = new ConcurrentBag<Assignment>();
            start.RegisterSymbolStartAction(
                typeStart => AnalyzeType(typeStart, stateMachine, allRegistrations, outsideAssignments),
                SymbolKind.NamedType);
            start.RegisterCompilationEndAction(end =>
            {
                foreach (var assignment in FindViolations(allRegistrations, outsideAssignments))
                {
                    end.ReportDiagnostic(Diagnostic.Create(AssignmentOutsideAggregate, assignment.Syntax.GetLocation(), assignment.Property.Name));
                }
            });
        });
    }

    // 绝大多数赋值写在聚合根自己的类型里，按类型分析，IDE 里就能马上提示。
    // 按类型分析不包括嵌套类型，类型之外的赋值留到编译结束再判断
    private static void AnalyzeType(
        SymbolStartAnalysisContext context,
        INamedTypeSymbol stateMachine,
        ConcurrentBag<Registration> allRegistrations,
        ConcurrentBag<Assignment> outsideAssignments)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        var registrations = new ConcurrentBag<Registration>();
        var assignments = new ConcurrentBag<Assignment>();

        context.RegisterOperationAction(
            c =>
            {
                if (FindRegistration(c, stateMachine) is { } registration)
                {
                    registrations.Add(registration);
                    allRegistrations.Add(registration);
                }
            },
            OperationKind.Invocation);
        context.RegisterOperationAction(
            c => CollectAssignments(c.Operation, assignment =>
            {
                var declaredHere = SymbolEqualityComparer.Default.Equals(assignment.Property.ContainingType.OriginalDefinition, type.OriginalDefinition);
                (declaredHere ? assignments : outsideAssignments).Add(assignment);
            }),
            OperationKind.SimpleAssignment,
            OperationKind.CompoundAssignment,
            OperationKind.CoalesceAssignment,
            OperationKind.DeconstructionAssignment,
            OperationKind.Increment,
            OperationKind.Decrement,
            OperationKind.PropertyInitializer);
        context.RegisterSymbolEndAction(c =>
        {
            foreach (var assignment in FindViolations(registrations, assignments))
            {
                c.ReportDiagnostic(Diagnostic.Create(DirectAssignment, assignment.Syntax.GetLocation(), assignment.Property.Name));
            }
        });
    }

    private static Registration? FindRegistration(OperationAnalysisContext context, INamedTypeSymbol stateMachine)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;
        if (!string.Equals(method.Name, DefineMethodName, StringComparison.Ordinal)
            || !SymbolEqualityComparer.Default.Equals(method.ContainingType, stateMachine))
        {
            return null;
        }

        // 按参数位置取，不按书写顺序：调用方可能用命名参数调换了顺序
        var getter = invocation.Arguments.FirstOrDefault(a => a.Parameter?.Ordinal == 0)?.Value;
        var setter = invocation.Arguments.FirstOrDefault(a => a.Parameter?.Ordinal == 1)?.Value;
        if (getter is null || setter is null)
        {
            return null;
        }

        if (FindDefinedProperty(getter) is not { } property)
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidGetter, getter.Syntax.GetLocation()));
            return null;
        }

        if (AsLambda(setter) is not { } setterLambda || !AssignsOnly(setterLambda, property))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidSetter, setter.Syntax.GetLocation()));
            return null;
        }

        CheckDeclaration(context, property);
        return new Registration(property, setterLambda.Syntax);
    }

    private static void CheckDeclaration(OperationAnalysisContext context, IPropertySymbol property)
    {
        var location = property.Locations.FirstOrDefault() ?? Location.None;
        if (property.SetMethod is not { DeclaredAccessibility: Accessibility.Private, IsInitOnly: false })
        {
            context.ReportDiagnostic(Diagnostic.Create(SetterNotPrivate, location, property.Name));
        }

        if (!IsAutoProperty(property, context.CancellationToken))
        {
            context.ReportDiagnostic(Diagnostic.Create(NotAutoProperty, location, property.Name));
        }
    }

    private static bool IsAutoProperty(IPropertySymbol property, CancellationToken ct) =>
        property.DeclaringSyntaxReferences.All(reference =>
            reference.GetSyntax(ct) is PropertyDeclarationSyntax { ExpressionBody: null, AccessorList: { } accessors }
            && accessors.Accessors.All(a => a is { Body: null, ExpressionBody: null }));

    // s => s.Status：只接受直接返回参数的某个属性。这个参数是表达式树，lambda 外面还包着一层转换
    private static IPropertySymbol? FindDefinedProperty(IOperation getter) =>
        AsLambda(getter) is { Symbol.Parameters: [var entity] } lambda
        && ExplicitStatements(lambda) is [IReturnOperation { ReturnedValue: { } returned }]
        && SkipConversions(returned) is IPropertyReferenceOperation reference
        && IsParameter(reference.Instance, entity)
            ? reference.Property.OriginalDefinition
            : null;

    // (s, v) => s.Status = v：只接受把第二个参数赋给同一个属性
    private static bool AssignsOnly(IAnonymousFunctionOperation setter, IPropertySymbol property) =>
        setter.Symbol.Parameters is [var entity, var value]
        && ExplicitStatements(setter) is [IExpressionStatementOperation { Operation: ISimpleAssignmentOperation assignment }]
        && assignment.Target is IPropertyReferenceOperation target
        && SymbolEqualityComparer.Default.Equals(target.Property.OriginalDefinition, property)
        && IsParameter(target.Instance, entity)
        && IsParameter(assignment.Value, value);

    // 表达式体 lambda 里编译器会补一个隐式 return，比较前先去掉
    private static IOperation[] ExplicitStatements(IAnonymousFunctionOperation lambda) =>
        [.. lambda.Body.Operations.Where(o => o is not IReturnOperation { ReturnedValue: null, IsImplicit: true })];

    private static IAnonymousFunctionOperation? AsLambda(IOperation operation) =>
        SkipConversions(operation) switch
        {
            IDelegateCreationOperation { Target: IAnonymousFunctionOperation lambda } => lambda,
            IAnonymousFunctionOperation lambda => lambda,
            _ => null,
        };

    private static bool IsParameter(IOperation? operation, IParameterSymbol parameter) =>
        operation is not null
        && SkipConversions(operation) is IParameterReferenceOperation reference
        && SymbolEqualityComparer.Default.Equals(reference.Parameter, parameter);

    private static IOperation SkipConversions(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        return operation;
    }

    private static void CollectAssignments(IOperation operation, Action<Assignment> add)
    {
        switch (operation)
        {
            // = null! 只是给 EF 用的占位，不算设置状态
            case IPropertyInitializerOperation initializer when IsPlaceholder(initializer.Value):
                break;

            case IPropertyInitializerOperation initializer:
                foreach (var property in initializer.InitializedProperties)
                {
                    add(new Assignment(property.OriginalDefinition, initializer.Syntax));
                }

                break;

            // 解构赋值也是 IAssignmentOperation，要先于下一个分支判断
            case IDeconstructionAssignmentOperation deconstruction:
                CollectTupleTargets(deconstruction.Target, add);
                break;

            case IAssignmentOperation assignment:
                CollectTarget(assignment.Target, add);
                break;

            case IIncrementOrDecrementOperation increment:
                CollectTarget(increment.Target, add);
                break;
        }
    }

    private static bool IsPlaceholder(IOperation value) =>
        SkipConversions(value) is ILiteralOperation { ConstantValue: { HasValue: true, Value: null } } or IDefaultValueOperation;

    private static void CollectTupleTargets(IOperation target, Action<Assignment> add)
    {
        if (target is not ITupleOperation tuple)
        {
            CollectTarget(target, add);
            return;
        }

        foreach (var element in tuple.Elements)
        {
            CollectTupleTargets(element, add);
        }
    }

    private static void CollectTarget(IOperation target, Action<Assignment> add)
    {
        if (target is IPropertyReferenceOperation reference)
        {
            add(new Assignment(reference.Property.OriginalDefinition, reference.Syntax));
        }
    }

    // 给已注册的状态属性赋值，并且不在它的 Define setter 里
    private static IEnumerable<Assignment> FindViolations(IEnumerable<Registration> registrations, IEnumerable<Assignment> assignments)
    {
        var settersByProperty = registrations.ToLookup(r => r.Property, SymbolEqualityComparer.Default);
        foreach (var assignment in assignments)
        {
            Registration[] setters = [.. settersByProperty[assignment.Property]];
            var insideSetter = setters.Any(r =>
                r.Setter.SyntaxTree == assignment.Syntax.SyntaxTree && r.Setter.Span.Contains(assignment.Syntax.Span));
            if (setters.Length > 0 && !insideSetter)
            {
                yield return assignment;
            }
        }
    }

    private sealed record Registration(IPropertySymbol Property, SyntaxNode Setter);

    private sealed record Assignment(IPropertySymbol Property, SyntaxNode Syntax);
}
