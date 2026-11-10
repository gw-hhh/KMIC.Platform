using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Testing.Assertions;
using Shouldly;

namespace Kmic.BuildingBlocks.Testing.Tests.Assertions;

public sealed class ResultAssertionsTests
{
    private static readonly Error _notFound = Error.NotFound("Thing.NotFound", "东西不存在");
    private static readonly Error _notDraft = Error.BusinessRule("Thing.NotDraft", "只有草稿可以修改");

    [Fact]
    public void ShouldBeSuccess_ReturnsValue()
    {
        Result<int> result = 42;

        result.ShouldBeSuccess().ShouldBe(42);
    }

    [Fact]
    public void ShouldBeSuccess_OnFailure_ReportsError()
    {
        Result result = _notFound;

        Should.Throw<ShouldAssertException>(result.ShouldBeSuccess).Message.ShouldContain("Thing.NotFound");
    }

    [Fact]
    public void ShouldBeFailure_MatchesCodeAndType()
    {
        Result result = _notDraft;

        result.ShouldBeFailure(_notDraft).ShouldBeSameAs(_notDraft);
        result.ShouldBeFailure(ErrorType.BusinessRule).ShouldBeSameAs(_notDraft);
        Should.Throw<ShouldAssertException>(() => result.ShouldBeFailure(_notFound)).Message.ShouldContain("Thing.NotDraft");
        Should.Throw<ShouldAssertException>(() => result.ShouldBeFailure(ErrorType.NotFound));
        Should.Throw<ShouldAssertException>(() => Result.Success().ShouldBeFailure(_notDraft)).Message.ShouldContain("实际成功");
    }
}
