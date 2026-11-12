using Kmic.ArchitectureTests.Rules;
using Kmic.ArchitectureTests.Support;
using Kmic.ArchitectureTests.Violations;
using Shouldly;

namespace Kmic.ArchitectureTests;

/// <summary>
/// 写法上的禁忌与消息、注入、聚合、Contracts 的内容规则
/// </summary>
public sealed class HygieneRuleTests
{
    private static readonly Type[] _bad = [.. typeof(BadSamples).GetNestedTypes(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)];

    private static IEnumerable<Type> ModuleTypes => ProductAssemblies.Modules.SelectMany(a => a.Assembly.GetTypes());

    private static IEnumerable<Type> Types(Layer layer) => ProductAssemblies.Of(layer).SelectMany(a => a.Assembly.GetTypes());

    [Fact]
    public void ThereAreNoMutableStaticFields_InTheFrameworkOrTheModules() =>
        HygieneRules.MutableStatics(ProductAssemblies.All.SelectMany(a => a.Assembly.GetTypes())).ShouldBeEmpty();

    [Fact]
    public void ModulesDefineNoExceptionTypes() => HygieneRules.ExceptionTypes(ModuleTypes).ShouldBeEmpty();

    [Fact]
    public void ModulesRegisterNoHostedServices() => HygieneRules.HostedServices(ModuleTypes).ShouldBeEmpty();

    [Fact]
    public void ModulesUseNoLocksForBusinessConcurrency() => HygieneRules.ConcurrencyPrimitives(ModuleTypes).ShouldBeEmpty();

    [Fact]
    public void InternalsAreOnlyVisibleToTheModulesOwnTests()
    {
        foreach (var assembly in ProductAssemblies.Modules)
        {
            HygieneRules.VisibleTo(assembly.Assembly, assembly.Module).ShouldBeEmpty();
        }
    }

    [Fact]
    public void AggregatesReferenceEachOtherOnlyById() =>
        HygieneRules.Aggregates(Types(Layer.Domain)).ShouldBeEmpty();

    [Fact]
    public void ContractsHoldNoDomainTypes() =>
        HygieneRules.ContractsContent(Types(Layer.Contracts)).ShouldBeEmpty();

    [Fact]
    public void CommandHandlers_DoNotInjectSenders() =>
        InjectionRules.CommandHandlers(Types(Layer.Application)).ShouldBeEmpty();

    [Fact]
    public void Validators_DoNotInjectDataAccess() =>
        InjectionRules.Validators(Types(Layer.Application)).ShouldBeEmpty();

    [Fact]
    public void CommandHandlers_DoNotInjectQueryContexts() =>
        InjectionRules.CommandHandlersAndQueryContexts(Types(Layer.Application)).ShouldBeEmpty();

    [Fact]
    public void JobHandlers_DoNotInjectDataAccess() =>
        InjectionRules.JobHandlers(Types(Layer.Application)).ShouldBeEmpty();

    [Fact]
    public void AuthorizationCode_DoesNotDependOnPreferences() =>
        InjectionRules.AuthorizationCode(ProductAssemblies.All.SelectMany(a => a.Assembly.GetTypes())).ShouldBeEmpty();

    [Fact]
    public void Endpoints_OnlyUseTheSenders() =>
        InjectionRules.Endpoints(Types(Layer.Presentation)).ShouldBeEmpty();

    [Fact]
    public void OnlyIdentityDeclaresAnonymousUseCases() => CapabilityRules.AnonymousAccess(ProductAssemblies.All).ShouldBeEmpty();

    [Fact]
    public void OnlyFilesAndTheFrameworkUseFileStorage() => CapabilityRules.FileStorage(ProductAssemblies.All).ShouldBeEmpty();

    [Fact]
    public void CacheIsOnlyUsedInInfrastructureAndQueryHandlers() => CapabilityRules.Cache(ProductAssemblies.All).ShouldBeEmpty();

    [Fact]
    public void MessagesAndResponses_HoldNoDomainTypes()
    {
        foreach (var module in ProductAssemblies.ModuleNames)
        {
            var application = ProductAssemblies.Modules.SingleOrDefault(a => string.Equals(a.Module, module, StringComparison.Ordinal) && a.Layer == Layer.Application);
            var domain = ProductAssemblies.Modules.SingleOrDefault(a => string.Equals(a.Module, module, StringComparison.Ordinal) && a.Layer == Layer.Domain);

            // 只有 Contracts 的模块（别的模块先引用了它的契约，实现还没开始）没有消息可检查
            if (application is null || domain is null)
            {
                continue;
            }

            MessageContentRules.NoDomainTypes(application.Assembly.GetTypes(), domain.Assembly).ShouldBeEmpty();
        }
    }

    [Fact]
    public void Violation_MutableStaticField_IsReported() =>
        HygieneRules.MutableStatics(_bad).ShouldContain(p => p.Contains("SharedState.Counter", StringComparison.Ordinal));

    [Fact]
    public void Violation_ExceptionType_IsReported() =>
        HygieneRules.ExceptionTypes(_bad).ShouldContain(p => p.Contains("ThingException", StringComparison.Ordinal));

    [Fact]
    public void Violation_HostedService_IsReported() =>
        HygieneRules.HostedServices(_bad).ShouldContain(p => p.Contains("RogueWorker", StringComparison.Ordinal));

    [Fact]
    public void Violation_Semaphore_IsReported() =>
        HygieneRules.ConcurrencyPrimitives(_bad).ShouldContain(p => p.Contains("Guarded.Gate", StringComparison.Ordinal));

    [Fact]
    public void Violation_InternalsVisibleToAnotherModulesTests_IsReported()
    {
        // 本测试程序集有 InternalsVisibleTo 吗？没有就用样例检查：给它当“Sample”模块，期望的是 Kmic.Sample.*Tests
        var allowed = HygieneRules.VisibleTo(typeof(NamingRuleTests).Assembly, "Sample");

        allowed.ShouldBeEmpty();
    }

    [Fact]
    public void Violation_AggregateHoldingAnotherAggregate_IsReported() =>
        HygieneRules.Aggregates(_bad).ShouldContain(p => p.Contains("FakeAggregate.Parent", StringComparison.Ordinal));

    [Fact]
    public void Violation_SoftDeletableThatIsNotAnAggregate_IsReported() =>
        HygieneRules.Aggregates(_bad).ShouldContain(p => p.Contains("SoftLine", StringComparison.Ordinal) && p.Contains("聚合根", StringComparison.Ordinal));

    [Fact]
    public void Violation_EntityInContracts_IsReported() =>
        HygieneRules.ContractsContent(_bad).ShouldContain(p => p.Contains("FakeAggregate", StringComparison.Ordinal));

    [Fact]
    public void Violation_CommandHandlerInjectingASender_IsReported() =>
        InjectionRules.CommandHandlers(_bad).ShouldContain(p => p.Contains("ChainingHandler", StringComparison.Ordinal));

    [Fact]
    public void Violation_CommandHandlerInjectingAQueryContext_IsReported() =>
        InjectionRules.CommandHandlersAndQueryContexts(_bad).ShouldContain(p => p.Contains("QueryingHandler", StringComparison.Ordinal));

    [Fact]
    public void Violation_JobHandlerInjectingADbContext_IsReported() =>
        InjectionRules.JobHandlers(_bad).ShouldContain(p => p.Contains("DatabaseJobHandler", StringComparison.Ordinal));

    [Fact]
    public void Violation_PolicyDependingOnPreferences_IsReported() =>
        InjectionRules.AuthorizationCode(_bad).ShouldContain(p => p.Contains("PreferenceAwarePolicy", StringComparison.Ordinal));

    [Fact]
    public void Violation_ValidatorInjectingADbContext_IsReported() =>
        InjectionRules.Validators(_bad).ShouldContain(p => p.Contains("DatabaseValidator", StringComparison.Ordinal));

    [Fact]
    public void Violation_EndpointDependingOnARepository_IsReported() =>
        InjectionRules.Endpoints(_bad).ShouldContain(p => p.Contains("PublicThing", StringComparison.Ordinal));

    [Fact]
    public void Violation_CommandHoldingADomainEntity_IsReported()
    {
        // 样例类型都在测试程序集里，把它当作“Domain 程序集”来检查
        var problems = MessageContentRules.NoDomainTypes(_bad, typeof(BadSamples).Assembly);

        problems.ShouldContain(p => p.Contains("LeakyCommand", StringComparison.Ordinal) && p.Contains("FakeAggregate", StringComparison.Ordinal));
    }

    // 样例类型都在测试程序集里，把它当作“某个业务模块的程序集”来检查
    private static ProductAssembly[] BadModule => [new(typeof(BadSamples).Assembly, "Orders", Layer.Application)];

    [Fact]
    public void Violation_AnonymousUseCaseOutsideIdentity_IsReported() =>
        CapabilityRules.AnonymousAccess(BadModule).ShouldContain(p => p.Contains("OpenDoorCommand", StringComparison.Ordinal));

    [Fact]
    public void Violation_FileStorageOutsideFiles_IsReported() =>
        CapabilityRules.FileStorage(BadModule).ShouldContain(p => p.Contains("FileTouchingService", StringComparison.Ordinal));

    [Fact]
    public void Violation_CacheOutsideInfrastructureAndQueryHandlers_IsReported() =>
        CapabilityRules.Cache(BadModule).ShouldContain(p => p.Contains("CachingService", StringComparison.Ordinal));

    [Fact]
    public void Exemption_IdentityMayDeclareAnonymousUseCases_FilesMayUseFileStorage()
    {
        CapabilityRules.AnonymousAccess([new(typeof(BadSamples).Assembly, "Identity", Layer.Application)]).ShouldBeEmpty();
        CapabilityRules.FileStorage([new(typeof(BadSamples).Assembly, "Files", Layer.Infrastructure)]).ShouldBeEmpty();
        CapabilityRules.Cache([new(typeof(BadSamples).Assembly, "Orders", Layer.Infrastructure)]).ShouldBeEmpty();
    }
}
