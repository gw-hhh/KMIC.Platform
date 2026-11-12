using Kmic.ArchitectureTests.Rules;
using Kmic.ArchitectureTests.Support;
using Kmic.ArchitectureTests.Violations;
using Shouldly;

namespace Kmic.ArchitectureTests;

/// <summary>
/// 命名与形态：命令、查询、处理器、校验器、端点、领域事件、强类型 ID、错误目录
/// </summary>
public sealed class NamingRuleTests
{
    private static readonly Type[] _bad = [.. typeof(BadSamples).GetNestedTypes(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)];

    private static IEnumerable<Type> Types(Layer layer) => ProductAssemblies.Of(layer).SelectMany(a => a.Assembly.GetTypes());

    [Fact]
    public void Messages_AreNamedAndShapedAsAgreed() =>
        NamingRules.Messages(Types(Layer.Application)).ShouldBeEmpty();

    [Fact]
    public void Handlers_ArePublicSealedAndNamedHandler() =>
        NamingRules.Handlers(Types(Layer.Application)).ShouldBeEmpty();

    [Fact]
    public void ValidatorsAndEndpoints_AreInternalSealedAndNamed()
    {
        NamingRules.InternalParts(Types(Layer.Application)).ShouldBeEmpty();
        NamingRules.InternalParts(Types(Layer.Presentation)).ShouldBeEmpty();
    }

    [Fact]
    public void DomainEvents_AreSealedRecords() =>
        NamingRules.DomainEvents(Types(Layer.Domain)).ShouldBeEmpty();

    [Fact]
    public void StronglyTypedIds_EndWithIdOrRef() =>
        NamingRules.Identifiers(Types(Layer.Domain)).ShouldBeEmpty();

    [Fact]
    public void ErrorCatalogs_AreNamedErrors_AndCodesStartWithTheModuleName()
    {
        foreach (var module in ProductAssemblies.ModuleNames)
        {
            var domain = ProductAssemblies.Modules.Where(a => string.Equals(a.Module, module, StringComparison.Ordinal) && a.Layer == Layer.Domain);
            NamingRules.Errors(domain.SelectMany(a => a.Assembly.GetTypes()), module).ShouldBeEmpty();
        }

        NamingRules.Errors(ProductAssemblies.Framework.Where(a => a.Layer == Layer.Domain).SelectMany(a => a.Assembly.GetTypes()), "BuildingBlocks").ShouldBeEmpty();
    }

    [Fact]
    public void Violation_CommandNamedWrongAndNotARecord_IsReported()
    {
        var problems = NamingRules.Messages(_bad);

        problems.ShouldContain(p => p.Contains("SubmitThing", StringComparison.Ordinal) && p.Contains("Command 结尾", StringComparison.Ordinal));
        problems.ShouldContain(p => p.Contains("SubmitThing", StringComparison.Ordinal) && p.Contains("sealed record", StringComparison.Ordinal));
    }

    [Fact]
    public void Violation_QueryNamedWrong_IsReported() =>
        NamingRules.Messages(_bad).ShouldContain(p => p.Contains("GetThing", StringComparison.Ordinal) && p.Contains("Query 结尾", StringComparison.Ordinal));

    [Fact]
    public void Violation_InternalHandlerWithWrongName_IsReported()
    {
        var problems = NamingRules.Handlers(_bad);

        problems.ShouldContain(p => p.Contains("SubmitThingRunner", StringComparison.Ordinal) && p.Contains("public sealed", StringComparison.Ordinal));
        problems.ShouldContain(p => p.Contains("SubmitThingRunner", StringComparison.Ordinal) && p.Contains("Handler 结尾", StringComparison.Ordinal));
    }

    [Fact]
    public void Violation_PublicValidatorAndEndpoint_AreReported()
    {
        var problems = NamingRules.InternalParts(_bad);

        problems.ShouldContain(p => p.Contains("DatabaseValidator", StringComparison.Ordinal) && p.Contains("internal sealed", StringComparison.Ordinal));
        problems.ShouldContain(p => p.Contains("PublicThing", StringComparison.Ordinal) && p.Contains("internal sealed", StringComparison.Ordinal));
        problems.ShouldContain(p => p.Contains("PublicThing", StringComparison.Ordinal) && p.Contains("Endpoint 结尾", StringComparison.Ordinal));
    }

    [Fact]
    public void Violation_DomainEventThatIsNotARecord_IsReported() =>
        NamingRules.DomainEvents(_bad).ShouldContain(p => p.Contains("ThingHappened", StringComparison.Ordinal));

    [Fact]
    public void Violation_IdWithoutIdSuffix_IsReported() =>
        NamingRules.Identifiers(_bad).ShouldHaveSingleItem().ShouldContain("Widget");

    [Fact]
    public void Violation_ErrorHolderWithWrongPrefix_IsReported()
    {
        var problems = NamingRules.Errors(_bad, "Sample");

        problems.ShouldContain(p => p.Contains("ErrorHolder", StringComparison.Ordinal) && p.Contains("Errors 结尾", StringComparison.Ordinal));
        problems.ShouldContain(p => p.Contains("Wrong.Thing", StringComparison.Ordinal) && p.Contains("Sample.", StringComparison.Ordinal));
    }
}
