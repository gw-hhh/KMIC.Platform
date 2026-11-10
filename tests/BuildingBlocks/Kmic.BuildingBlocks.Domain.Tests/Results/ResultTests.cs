using Kmic.BuildingBlocks.Domain.Results;
using Shouldly;

namespace Kmic.BuildingBlocks.Domain.Tests.Results;

public sealed class ResultTests
{
    private static readonly Error _notFound = Error.NotFound("Test.NotFound", "不存在");
    private static readonly Error _rule = Error.BusinessRule("Test.Rule", "违反规则");

    [Fact]
    public void Value_WhenFailure_Throws()
    {
        Result<int> result = _notFound;

        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Error_WhenSuccess_Throws()
    {
        Should.Throw<InvalidOperationException>(() => Result.Success().Error);
    }

    [Fact]
    public void ImplicitConversion_FromValueAndError_SetsState()
    {
        Result<int> success = 42;
        Result<int> failure = _notFound;
        Result plainFailure = _rule;

        success.IsSuccess.ShouldBeTrue();
        success.Value.ShouldBe(42);
        failure.IsFailure.ShouldBeTrue();
        failure.Error.ShouldBeSameAs(_notFound);
        plainFailure.Error.ShouldBeSameAs(_rule);
    }

    [Fact]
    public void MapAndBind_WhenFailure_ShortCircuit()
    {
        Result<int> failure = _notFound;
        var called = false;

        var mapped = failure.Map(x => { called = true; return x * 2; });
        var bound = failure.Bind<int>(x => { called = true; return x; });

        called.ShouldBeFalse();
        mapped.Error.ShouldBeSameAs(_notFound);
        bound.Error.ShouldBeSameAs(_notFound);
    }

    [Fact]
    public void MapAndBind_WhenSuccess_ContinueWithValue()
    {
        Result<int> success = 21;

        success.Map(x => x * 2).Value.ShouldBe(42);
        success.Bind<string>(_ => _rule).Error.ShouldBeSameAs(_rule);
    }

    [Fact]
    public void Match_PicksBranchByOutcome()
    {
        Result<int> success = 1;
        Result failure = _rule;

        success.Match(x => $"值 {x}", e => e.Code).ShouldBe("值 1");
        failure.Match(() => "成功", e => e.Code).ShouldBe("Test.Rule");
    }

    [Fact]
    public void FirstFailure_ReturnsFirstFailedOrSuccess()
    {
        Result<int> ok = 1;
        Result<int> first = _notFound;
        Result<int> second = _rule;

        Result.FirstFailure(ok, first, second).Error.ShouldBeSameAs(_notFound);
        Result.FirstFailure(ok, Result.Success()).IsSuccess.ShouldBeTrue();
    }
}
