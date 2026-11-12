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
    public void Endpoints_OnlyUseTheSenders() =>
        InjectionRules.Endpoints(Types(Layer.Presentation)).ShouldBeEmpty();

    [Fact]
    public void MessagesAndResponses_HoldNoDomainTypes()
    {
        foreach (var module in ProductAssemblies.ModuleNames)
        {
            var application = ProductAssemblies.Modules.Single(a => string.Equals(a.Module, module, StringComparison.Ordinal) && a.Layer == Layer.Application);
            var domain = ProductAssemblies.Modules.Single(a => string.Equals(a.Module, module, StringComparison.Ordinal) && a.Layer == Layer.Domain);

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
}
