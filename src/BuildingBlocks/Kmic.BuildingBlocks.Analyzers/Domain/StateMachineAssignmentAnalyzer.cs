using System.Collections.Concurrent;
using System.Collections.Immutable;
using Kmic.BuildingBlocks.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Kmic.BuildingBlocks.Analyzers.Domain;

/// <summary>KM0401：状态属性仅允许由状态机修改。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StateMachineAssignmentAnalyzer : DiagnosticAnalyzer
{
    internal const string DiagnosticId = "KM0401";

    private const string Title = "状态属性只能由状态机修改";
    private const string Reason = "直接赋值会绕过状态机的流转校验，且无法记录状态流转。";
    private const string DefineMethodName = "Define";

    internal static readonly DiagnosticDescriptor DirectAssignment = Rules.Create(
        DiagnosticId,
        Rules.DomainCategory,
        Title,
        "禁止直接为状态属性 {0} 赋值，请改用状态机的 Start 或 Fire",
        Reason);

    // 需待编译结束才能与 Define 匹配，IDE 默认不实时提示，构建时生效
    internal static readonly DiagnosticDescriptor AssignmentOutsideAggregate = Rules.Create(
        DiagnosticId,
        Rules.DomainCategory,
        Title,
        "禁止在其他类型中为状态属性 {0} 赋值，请改用状态机的 Start 或 Fire",
        Reason,
        WellKnownDiagnosticTags.CompilationEnd);

    internal static readonly DiagnosticDescriptor SetterNotPrivate = Rules.Create(
        DiagnosticId,
        Rules.DomainCategory,
        Title,
        "状态属性 {0} 的 setter 必须为 private",
        Reason);

    internal static readonly DiagnosticDescriptor NotAutoProperty = Rules.Create(
        DiagnosticId,
        Rules.DomainCategory,
        Title,
        "状态属性 {0} 必须声明为自动属性",
        "若存在显式支持字段，类型内部代码可能绕过属性直接修改字段，导致静态检查失效。");

    internal static readonly DiagnosticDescriptor InvalidGetter = Rules.Create(
        DiagnosticId,
        Rules.DomainCategory,
        Title,
        "Define 的首个参数只能直接返回聚合根的状态属性（如 s => s.Status）",
        "分析器依赖该表达式识别状态属性，使用其他形式将无法正确识别。");

    internal static readonly DiagnosticDescriptor InvalidSetter = Rules.Create(
        DiagnosticId,
        Rules.DomainCategory,
        Title,
        "Define 的第二个参数只能向首个参数指定的属性赋值（如 (s, v) => s.Status = v）",
        "状态机仅通过该委托写入状态值；若在其中修改其他属性，将绕过业务方法约束。");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [DirectAssignment, AssignmentOutsideAggregate, SetterNotPrivate, NotAutoProperty, InvalidGetter, InvalidSetter];

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

    // 绝大多数赋值位于聚合根自身类型内部，按类型粒度分析即可在 IDE 中实时提示；
    // 跨类型（含嵌套类型）的赋值则延后至编译结束统一匹配检查
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

        // 按参数位置而非实参书写顺序匹配，兼容调用方使用命名参数调换顺序的情况
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

    // 仅接受直接访问参数属性的表达式（如 s => s.Status）；因参数类型为表达式树，Lambda 外层包裹了一层转换操作
    private static IPropertySymbol? FindDefinedProperty(IOperation getter) =>
        AsLambda(getter) is { Symbol.Parameters: [var entity] } lambda
        && ExplicitStatements(lambda) is [IReturnOperation { ReturnedValue: { } returned }]
        && SkipConversions(returned) is IPropertyReferenceOperation reference
        && IsParameter(reference.Instance, entity)
            ? reference.Property.OriginalDefinition
            : null;

    // 仅接受将第二个参数直接赋给目标属性，如 (s, v) => s.Status = v
    private static bool AssignsOnly(IAnonymousFunctionOperation setter, IPropertySymbol property) =>
        setter.Symbol.Parameters is [var entity, var value]
        && ExplicitStatements(setter) is [IExpressionStatementOperation { Operation: ISimpleAssignmentOperation assignment }]
        && assignment.Target is IPropertyReferenceOperation target
        && SymbolEqualityComparer.Default.Equals(target.Property.OriginalDefinition, property)
        && IsParameter(target.Instance, entity)
        && IsParameter(assignment.Value, value);

    // 表达式体 Lambda 会由编译器生成隐式 return 操作，匹配前先予过滤
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
            // = null! 仅作 EF 占位，不视为状态赋值
            case IPropertyInitializerOperation initializer when IsPlaceholder(initializer.Value):
                break;

            case IPropertyInitializerOperation initializer:
                foreach (var property in initializer.InitializedProperties)
                {
                    add(new Assignment(property.OriginalDefinition, initializer.Syntax));
                }

                break;

            // 解构赋值同样实现 IAssignmentOperation，需先于通用赋值分支匹配
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

    // 匹配向已注册状态属性赋值且位于对应 Define setter 之外的操作
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
