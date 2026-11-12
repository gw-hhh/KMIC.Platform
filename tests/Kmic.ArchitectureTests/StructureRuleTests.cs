using Kmic.ArchitectureTests.Rules;
using Kmic.ArchitectureTests.Support;
using Kmic.ArchitectureTests.Violations;
using Shouldly;

namespace Kmic.ArchitectureTests;

/// <summary>
/// 命令响应、处理器引用、领域事件处理器、服务注册、聚合初始化
/// </summary>
public sealed class StructureRuleTests
{
    private static readonly Type[] _bad = [.. typeof(BadSamples).GetNestedTypes(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)];

    private static IEnumerable<Type> AllTypes => ProductAssemblies.All.SelectMany(a => a.Assembly.GetTypes());

    private static IEnumerable<Type> ModuleTypes => ProductAssemblies.Modules.SelectMany(a => a.Assembly.GetTypes());

    [Fact]
    public void CommandResponses_AreReceipts() => StructureRules.CommandResponses(AllTypes).ShouldBeEmpty();

    [Fact]
    public void HandlersAreOnlyReferencedByTheSenders() => StructureRules.HandlerReferences(AllTypes).ShouldBeEmpty();

    [Fact]
    public void DomainEventHandlers_OnlyDependOnTheirOwnModule() =>
        StructureRules.DomainEventHandlers(ModuleTypes, StructureRules.ModuleOf).ShouldBeEmpty();

    [Fact]
    public void ServiceRegistration_OnlyInTheModuleRegistrationClasses() => StructureRules.ServiceRegistration(ModuleTypes).ShouldBeEmpty();

    [Fact]
    public void EveryAggregate_InitializesItsStateMachine() =>
        StructureRules.AggregateInitialization(ProductAssemblies.Of(Layer.Domain, modulesOnly: false).SelectMany(a => a.Assembly.GetTypes())).ShouldBeEmpty();

    [Fact]
    public void Violation_CommandResponseWithANestedObjectOrBusinessCollection_IsReported()
    {
        var problems = StructureRules.CommandResponses(_bad);

        problems.ShouldContain(p => p.Contains("BadReceipt", StringComparison.Ordinal) && p.Contains("Detail", StringComparison.Ordinal));
        problems.ShouldContain(p => p.Contains("BadReceiptItems", StringComparison.Ordinal) && p.Contains("Items", StringComparison.Ordinal));
    }

    [Fact]
    public void CommandResponses_AllowReceiptsAndSystemOnlyCommands()
    {
        var problems = StructureRules.CommandResponses(_bad);

        problems.ShouldNotContain(p => p.Contains("GoodReceipt", StringComparison.Ordinal));
        problems.ShouldNotContain(p => p.Contains("SystemOutcomeCommand", StringComparison.Ordinal));
    }

    [Fact]
    public void Violation_TypeReferencingAHandler_IsReported() =>
        StructureRules.HandlerReferences(_bad).ShouldContain(p => p.Contains("HandlerUser", StringComparison.Ordinal) && p.Contains("ChainingHandler", StringComparison.Ordinal));

    [Fact]
    public void Violation_DomainEventHandlerInjectingAnotherModule_IsReported() =>
        StructureRules.DomainEventHandlers(_bad, type => type == typeof(BadSamples.IOtherModuleThing) ? "Other" : "Mine")
            .ShouldContain(p => p.Contains("BadEventHandler", StringComparison.Ordinal) && p.Contains("Other", StringComparison.Ordinal));

    [Fact]
    public void Violation_ServiceRegistrationOutsideTheModuleClass_IsReported() =>
        StructureRules.ServiceRegistration(_bad).ShouldContain(p => p.Contains("RogueRegistration", StringComparison.Ordinal));

    [Fact]
    public void Violation_AggregateWhoseStateMachineFailsToBuild_IsReported() =>
        StructureRules.AggregateInitialization(_bad).ShouldContain(p => p.Contains("BrokenAggregate", StringComparison.Ordinal));

    [Fact]
    public void SensitivePermissions_AreMarked() => StructureRules.SensitivePermissions(AllTypes).ShouldBeEmpty();

    [Fact]
    public void ReadModels_AreNotAggregateRoots() => StructureRules.ReadModels(AllTypes).ShouldBeEmpty();

    [Fact]
    public void Violation_PurgePermissionWithoutSensitive_IsReported()
    {
        var problems = StructureRules.SensitivePermissions(_bad);

        problems.ShouldHaveSingleItem().ShouldContain("Bad.Thing.Purge");
    }

    [Fact]
    public void Violation_ReadModelThatIsAnAggregateRoot_IsReported() =>
        StructureRules.ReadModels(_bad).ShouldContain(p => p.Contains("BadReadModel", StringComparison.Ordinal));

    [Fact]
    public void CreateSubmitApproveAndPayCommands_DeclareTheirIdempotency() => StructureRules.IdempotencyDeclarations(AllTypes).ShouldBeEmpty();

    [Fact]
    public void Validators_DoNotInjectModuleContracts() => StructureRules.ValidatorsAndContracts(ModuleTypes, StructureRules.IsModuleContracts).ShouldBeEmpty();

    [Fact]
    public void Violation_CreateCommandWithoutIdempotencyDeclaration_IsReported()
    {
        var problems = StructureRules.IdempotencyDeclarations(_bad);

        problems.ShouldContain(p => p.Contains("CreateThingCommand", StringComparison.Ordinal));
        problems.ShouldNotContain(p => p.Contains("SubmitThingAgainCommand", StringComparison.Ordinal));
    }

    [Fact]
    public void Violation_ValidatorInjectingContracts_IsReported() =>
        StructureRules.ValidatorsAndContracts(_bad, type => type == typeof(BadSamples.IThingReferences))
            .ShouldContain(p => p.Contains("ReferencingValidator", StringComparison.Ordinal));

    [Fact]
    public void WithoutDataScope_OnlyInMarkedHandlers() => StructureRules.DataScopeBypasses(ModuleTypes).ShouldBeEmpty();

    [Fact]
    public void Violation_UnmarkedHandlerAndHelperCallingWithoutDataScope_AreReported()
    {
        var problems = StructureRules.DataScopeBypasses(_bad);

        problems.ShouldContain(p => p.Contains("UnmarkedBypassHandler", StringComparison.Ordinal));
        problems.ShouldContain(p => p.Contains("ScopeHelper", StringComparison.Ordinal));
        problems.ShouldNotContain(p => p.Contains("MarkedBypassHandler", StringComparison.Ordinal));
    }

    [Fact]
    public void CachedValues_AreImmutable() => StructureRules.CacheValues(AllTypes).ShouldBeEmpty();

    [Fact]
    public void Violation_MutableCachedValue_IsReported()
    {
        var problems = StructureRules.CacheValues(_bad);

        problems.ShouldHaveSingleItem().ShouldContain("MutableValue");
    }
}
